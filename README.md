# StockAlerter

StockAlerter is a .NET application designed to monitor stock prices and send alerts based on predefined conditions. It leverages various libraries for email notifications, HTTP requests, and data parsing.

## Features
- Fetches stock data from Yahoo Finance API.
- Sends email alerts when stock conditions are met.
- Configurable settings via `appsettings.json`.

## Prerequisites
- .NET 9.0 SDK or later
- A valid email account for sending alerts

## Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/buerklma/GHStockalert.git
   ```
2. Navigate to the project directory:
   ```bash
   cd StockAlerter
   ```
3. Restore dependencies:
   ```bash
   dotnet restore
   ```

## Configuration
1. Open the `appsettings.json` file.
2. Update the configuration settings, such as email credentials and stock symbols to monitor.

## Usage
Run the application using the following command:
```bash
dotnet run
```

## Project Structure
- `Models/`: Contains data models like `Stock.cs`.
- `Services/`: Includes service interfaces and implementations for email and stock data handling.
- `appsettings.json`: Configuration file for the application.
- `Program.cs`: Entry point of the application.

## Contributing
Contributions are welcome! Please fork the repository and submit a pull request.

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.