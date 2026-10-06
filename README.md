# ConsoleStore

An ASP.NET Core e-commerce web application for PlayStation consoles and accessories, with a hardware extension: an Arduino with a TFT display that reacts to shopping cart events in real time.

## Features

- Product catalog with PlayStation consoles and accessories
- Shopping cart (add, remove, update quantities)
- Arduino + TFT display integration: the screen shows cart activity when items are added or removed
- Pixel-art graphics (PS5 and DualSense controller) rendered on the TFT screen
- User accounts and authentication
- Order checkout

## Tech Stack

- **Backend:** ASP.NET Core MVC , C#
- **Database:** SQL Server with Entity Framework Core
- **Frontend:** HTML, CSS, JavaScript, Bootstrap
- **Hardware:** Arduino UNO, TFT display , communication over Serial

## Project Structure

```
├── ConsoleStore.sln        # Visual Studio solution
└── ConsoleStore/
    ├── Controllers/        # MVC controllers (e.g. CartController)
    ├── Models/             # data models
    ├── Views/              # Razor views
    ├── wwwroot/            # static files (CSS, JS, images)
    └── Program.cs          # application entry point
```

## Getting Started

**Requirements:** .NET SDK 8.0, Visual Studio 2022 or VS Code, SQL Server.

```bash
git clone https://github.com/Bughi24/ConsoleStore.git
cd ConsoleStore
dotnet restore
```

Configure the connection string in `ConsoleStore/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your_connection_string_here"
  }
}
```

Apply database migrations and run the app:

```bash
dotnet ef database update --project ConsoleStore
dotnet run --project ConsoleStore
```

The application will be available at `https://localhost:[port]`.

## Arduino Setup

1. Connect the TFT display to the Arduino pin configuration.
2. Upload the sketch from `[arduino folder]` using the Arduino IDE.
3. Set the serial port in the application configuration.
4. Start the web app. Adding or removing a product from the cart updates the display.

## Author

**Calafeteanu Bogdan-Ștefan** - Faculty of Automation, Computers and Electronics, University of Craiova.

## License

Academic project, all rights reserved.
