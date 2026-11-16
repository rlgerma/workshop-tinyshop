# TinyShop

Starter solution for the GitHub Copilot workshop in Visual Studio 2026. The solution includes a .NET Aspire host, Minimal API product service, Blazor frontend, and tests.

## Start the workshop

```powershell
git clone https://github.com/jamesmontemagno/workshop-tinyshop.git
cd workshop-tinyshop
start src\TinyShop.sln
```

Open `src\TinyShop.sln` in Visual Studio 2026 and run **TinyShop.AppHost**.

`main` contains the intentionally incomplete starting point used by the workshop. To inspect the finished solution, switch to the `completed` branch:

```powershell
git switch completed
start src\TinyShop.sln
```

## Build

Install the .NET 10 or .NET 11 SDK, then run:

```powershell
dotnet build src\TinyShop.sln
```

Package restore requires access to NuGet.org, which is configured in the repository's `NuGet.Config`.

The app targets .NET 10 and rolls forward to a newer major runtime if .NET 10 is not installed.

## Products API

The Products service exposes these routes under `/api/Product`:

- `GET /` lists products; `GET /{id}` retrieves one product.
- `POST /` creates a product and returns `201 Created` with its generated ID and a `Location` header. Any ID supplied in the body is ignored.
- `PUT /{id}` replaces the product's name, description, price, and image URL and returns `204 No Content`. The route ID identifies the product; the body ID is ignored.
- `DELETE /{id}` removes a product and returns `204 No Content`.

The individual GET, PUT, and DELETE routes return `404 Not Found` when the product does not exist. The service currently uses an in-memory database, so changes are lost when it restarts.

## License

MIT. The original workshop source and license are preserved from [dotnet-presentations/visual-studio-github-copilot-lab](https://github.com/dotnet-presentations/visual-studio-github-copilot-lab).
