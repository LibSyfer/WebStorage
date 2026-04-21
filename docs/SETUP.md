# SETUP: Инструкция по установке и запуску WebStorage API

## 📋 Содержание

- [Требования](#-требования)
- [Быстрый старт](#-быстрый-старт)
- [Конфигурация](#-конфигурация)
- [Запуск](#-запуск)
- [Проверка](#-проверка)
- [Решение проблем](#-решение-проблем)

---

## ✅ Требования

### Системные требования

- **OS**: Windows, macOS, Linux
- **.NET SDK**: 10.0 или выше
- **Git**: для клонирования репозитория

### Проверка установки

```powershell
# Проверить версию .NET
dotnet --version

# Должно быть >= 10.0
```

---

## 🚀 Быстрый старт

### 1. Клонируйте репозиторий

```bash
git clone https://github.com/LibSyfer/WebStorage.git
cd WebStorage
```

### 2. Восстановите зависимости

```bash
dotnet restore
```

### 3. Примените миграции БД

```bash
# Перейдите в папку API проекта
cd src/WebStorage.Api

# Примените миграции (если используется Entity Framework)
dotnet ef database update
```

### 4. Запустите приложение

```bash
# Из папки проекта
dotnet run

# Или
dotnet build
dotnet bin/Debug/net10.0/WebStorage.Api.dll
```

**Вывод:**
```
info: WebStorage.Api.Program[0]
      Now listening on: https://localhost:7001
info: WebStorage.Api.Program[0]
      Application started. Press Ctrl+C to stop.
```

### 5. Откройте в браузере

- **API**: https://localhost:7001
- **Swagger UI**: https://localhost:7001/swagger/ui (если настроено)
- **OpenAPI Schema**: https://localhost:7001/openapi/v1.json

---

## ⚙️ Конфигурация

### appsettings.json

Основной файл конфигурации находится в `src/WebStorage.Api/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json

Для разработки (`src/WebStorage.Api/appsettings.Development.json`):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

### Переменные окружения

Используйте переменные окружения для конфиденциальных данных:

```bash
# PowerShell
$env:JWT__SECRET = "your-super-secret-key-min-32-chars"
$env:JWT__ISSUER = "WebStorage"
$env:JWT__AUDIENCE = "WebStorageUsers"

# bash
export JWT__SECRET="your-super-secret-key-min-32-chars"
export JWT__ISSUER="WebStorage"
export JWT__AUDIENCE="WebStorageUsers"

# Windows CMD
set JWT__SECRET=your-super-secret-key-min-32-chars
set JWT__ISSUER=WebStorage
set JWT__AUDIENCE=WebStorageUsers
```

### appsettings.Production.json

Для production окружения:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  },
  "Kestrel": {
    "EndpointDefaults": {
      "Protocols": "Http2"
    }
  }
}
```

---

## 🎯 Запуск

### Development (разработка)

```bash
dotnet run
```

**Параметры:**
- Автоматическая перезагрузка при изменении файлов (Hot Reload)
- Подробные логи
- Swagger документация включена

### Production (продакшн)

```bash
# Сборка
dotnet publish -c Release -o ./publish

# Запуск
cd publish
dotnet WebStorage.Api.dll

# Или как сервис Windows
# См. раздел "Запуск как сервис Windows"
```

### Debug режим в Visual Studio

```
1. Открыть WebStorage.sln в Visual Studio
2. Выбрать WebStorage.Api как Startup Project
3. Нажать F5 (или Ctrl+F5)
4. Visual Studio откроет приложение в браузере
```

### Запуск конкретного профиля

```bash
# Список доступных профилей
dotnet run --list-profiles

# Запустить с профилем
dotnet run --launch-profile https
```

---

## ✔️ Проверка

### 1. Проверить, что приложение запущено

```bash
curl https://localhost:7001/

# Ожидается:
# Welcome to the WebStore!
```

### 2. Проверить OpenAPI

```bash
curl https://localhost:7001/openapi/v1.json
```

### 3. Тестовая регистрация

```bash
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "TestPassword123!"
  }'
```

Ожидаемый ответ:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "test@example.com",
    "roles": ["User"]
  }
}
```

### 4. Протестировать protected endpoint

```bash
# Используйте accessToken из предыдущего ответа
curl -X GET https://localhost:7001/api/storage/me \
  -H "Authorization: Bearer {accessToken}"
```

---

## 🐛 Решение проблем

### Ошибка: "Порт уже занят"

```
error: An attempt was made to access a socket in a way forbidden by its access restrictions
```

**Решение:**

```bash
# Найдите процесс, использующий порт 7001
netstat -ano | findstr :7001

# Завершите процесс (Windows)
taskkill /PID {PID} /F

# Или запустите на другом порту
dotnet run --urls "https://localhost:7002"
```

### Ошибка: "SSL Certificate"

```
System.Net.Http.HttpRequestException: The SSL connection could not be established
```

**Решение:**

```bash
# Установите самоподписанный сертификат (для development)
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# Или запустите без HTTPS
dotnet run --urls "http://localhost:5000"
```

### Ошибка: "Database connection failed"

```
System.Data.SqlClient.SqlException: A network-related or instance-specific error
```

**Решение:**

```bash
# Проверьте строку подключения в appsettings.json
# Убедитесь, что БД запущена и доступна

# Попробуйте создать БД с нуля
dotnet ef database drop --force
dotnet ef database update
```

### Ошибка: ".NET SDK not found"

```
A compatible .NET SDK version could not be found
```

**Решение:**

```bash
# Установите .NET 10 SDK с https://dotnet.microsoft.com/download

# Проверьте версию после установки
dotnet --version

# Если несколько версий, укажите в global.json
# (если файл существует)
```

### Ошибка при миграции БД

```
The Entity Framework tools version '7.0.0' is newer than the runtime version '6.0.0'
```

**Решение:**

```bash
# Обновите Entity Framework tools
dotnet tool update --global dotnet-ef

# Или переустановите
dotnet tool uninstall --global dotnet-ef
dotnet tool install --global dotnet-ef
```

### Свежий старт (очистить всё)

```bash
# Удалить артефакты сборки
dotnet clean

# Удалить зависимости
rm -r ./packages
rm -r ./.nuget

# Восстановить
dotnet restore

# Переразобрать
dotnet build

# Запустить миграции
dotnet ef database drop --force
dotnet ef database update

# Запустить
dotnet run
```

---

## 🔒 Безопасность при запуске

### Для Development

```bash
# ✅ Используйте самоподписанные сертификаты
dotnet dev-certs https --trust
```

### Для Production

```bash
# ✅ Используйте Let's Encrypt или коммерческий сертификат
# ✅ Установите строгие переменные окружения
# ✅ Используйте HTTPS обязательно
# ✅ Установите правильные CORS headers
# ✅ Включите Rate Limiting
```

---

## 📊 Логирование

### Уровни логирования

```
Trace      = 0 - Самые подробные сообщения
Debug      = 1 - Сообщения отладки
Information = 2 - Информационные сообщения (по умолчанию)
Warning    = 3 - Предупреждения
Error      = 4 - Ошибки
Critical   = 5 - Критические ошибки
```

### Настройка в appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Debug",
      "WebStorage": "Debug"
    }
  }
}
```

### Просмотр логов

```bash
# Логи выводятся в консоль при запуске
dotnet run

# Можно перенаправить в файл
dotnet run > logs.txt 2>&1
```

---

## 🚢 Развёртывание (Deployment)

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80 443
ENTRYPOINT ["dotnet", "WebStorage.Api.dll"]
```

Запуск:
```bash
docker build -t webstorage-api .
docker run -p 8080:80 webstorage-api
```

### IIS

```bash
# Опубликовать для IIS
dotnet publish -c Release -o C:\inetpub\wwwroot\webstorage-api

# Настроить Application Pool для .NET
# См. https://docs.microsoft.com/aspnet/core/host-and-deploy/iis/
```

### Azure App Service

```bash
# Установите Azure CLI
# https://docs.microsoft.com/cli/azure/install-azure-cli

az login

# Создайте App Service
az appservice plan create --name webstorage-plan --resource-group mygroup --sku B1

az webapp create --resource-group mygroup \
  --plan webstorage-plan \
  --name webstorage-api \
  --runtime "DOTNETCORE|10.0"

# Опубликуйте
dotnet publish -c Release -o ./publish
cd publish
zip -r app.zip *
az webapp deployment source config-zip --resource-group mygroup \
  --name webstorage-api \
  --src app.zip
```

---

## 📚 Дополнительно

- **API Documentation**: `API.md`
- **Authentication**: `AUTHENTICATION.md`
- **GitHub**: https://github.com/LibSyfer/WebStorage
- **официальная документация .NET**: https://docs.microsoft.com/dotnet/

