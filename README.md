# Order Management Service

 **Cистема управления заказами. Это проект, построенный на микросервисной архитектуре и предоставляющий пользователю возможность регистрироваться в системе, заказывать, публиковать и отслеживать товары, а также управлять своим профилем и балансом.**

# Сервисы

- **`AuthService`**
  - **Назначение**: логин/логаут, выдача и обновление JWT/refresh token.
  - **Выходы/интеграции**: читает роли пользователя из CustomerService

- **`CustomerService`**
  - **Назначение**: регистрация пользователя, хранение пользователей и ролей, выдача ролей (admin-only).
  - **Выходы/интеграции**: публикует Kafka событие `customer-registered`.

- **`CatalogService`**
  - **Назначение**: хранение информаци о категориях, товарах, цена, доступности, количестве (`Stock`).
  - **Выходы/интеграции**: публикует `product-upserted`; слушает `order-paid` и списывает склад.

- **`OrderService`**
  - **Назначение**: создание и управление заказами.

  - **Выходы/интеграции**: публикует `order-payment-requested`; слушает `order-paid`/`order-payment-failed` и обновляет статус заказа.

- **`PaymentService`**
  - **Назначение**: храние баланса пользователя, пополнение/списание, обработка оплаты заказа
  - **Выходы/интеграции**: слушает `customer-registered` (создаёт баланс), слушает `order-payment-requested` (пытается списать), публикует `order-paid` или `order-payment-failed`.

# Технологии

- **.NET / ASP.NET Core**
- **Entity Framework Core** + миграции
- **PostgreSQL** (отдельная БД на сервис)
- **Kafka** (Confluent.Kafka) для событий
- **AuttoMaper**
- **MediatR (CQRS)** (используется в сервисах на уровне Application)
- **FluentValidation** (валидация команд/запросов)
- **JWT Bearer Authentication** (авторизация в сервисах)
- **Docker Compose** (Kafka/Zookeeper и Postgres-контейнеры)
- **Unit, Moq** (Юнит тестирование)

# События в Kafka

- **`customer-registered`**
  - **Producer**: `CustomerService`
  - **Consumers**: `AuthService`, `PaymentService`

- **`product-upserted`**
  - **Producer**: `CatalogService`
  - **Consumer**: `OrderService` (обновляет `CatalogProductSnapshot`)

- **`order-payment-requested`**
  - **Producer**: `OrderService`
  - **Consumer**: `PaymentService`

- **`order-paid`**
  - **Producer**: `PaymentService`
  - **Consumers**: `OrderService` (ставит статус Paid), `CatalogService` (списывает `Stock`)

- **`order-payment-failed`**
  - **Producer**: `PaymentService`
  - **Consumer**: `OrderService` (отменяет заказ)

# Основные эндпоинты

## AuthService

- **POST** `/api/auth/login`
- **POST** `/api/auth/refresh`
- **POST** `/api/auth/logout`

## CustomerService

- **POST** `/api/customers/register` *(публичный)*
- **GET** `/api/customers/{id}` *(Admin)*
- **GET** `/api/customers/by-email/{email}` *(Admin)*
- **POST** `/api/customers/{id}/roles` *(Admin)* — выдать роль

## CatalogService

- **POST** `/api/category` *(Admin)* — создать категорию
- **GET** `/api/category` — список категорий
- **POST** `/api/product` *(Admin)* — создать товар
- **PUT** `/api/product/{id}` *(Admin)* — обновить товар
- **GET** `/api/product/{id}` — товар по id
- **GET** `/api/product` — список товаров

## OrderService

- **POST** `/api/order` — создать заказ (создаётся со статусом `PendingPayment`)
- **GET** `/api/order/{id}` — получить заказ
- **GET** `/api/order/my` — мои заказы
- **POST** `/api/order/{id}/cancel` — отменить
- **POST** `/api/order/{id}/ship` — отгрузить

## PaymentService

- **GET** `/api/balances/{customerId}` — получить баланс
- **POST** `/api/balances/{customerId}/credit` — пополнить
- **POST** `/api/balances/{customerId}/debit` — списать

# Быстрый запуск

- **Инфраструктура**: `docker compose up -d`
- **Сборка**: `dotnet build OrderManagement.sln`
- **Запуск сервисов**: `dotnet run --project <path-to-csproj>`
