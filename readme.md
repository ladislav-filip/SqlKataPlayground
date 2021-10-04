# SqlKata samples
Ukázky použití Query Builderu [SqlKata](https://github.com/sqlkata/querybuilder)

## Požadavky
Pro spuštění projektu je potřeba mít k dispozici MySQL databázi. Toto je vyřešeno pomocí technologie [Docker](https://www.docker.com/).
Kontejner se spustí příkazem ```docker-compose up -d``` v adresáři "./SqlKataMySql/Docker".

### Naplnění testovacími daty
Při prvním spuštění aplikace je potřeba vytvořit databázi s testovacími daty. K tomu slouží třída [Seeder](src/SqlKataMySql/Persistence/Seeder.cs).
 Zapnutí/vypnutí naplnění databáze se provede ve třídě [HostExtensions](src/SqlKataMySql/Extensions/HostExtensions.cs) v metodě **RunConsoleAsync**:
```csharp
var seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
await seeder.SeedAsync();
```

## Example 1
Inicializace projektu a jednoduché dotazy na tabulkou adres.

## Example 2
* ukázka provádění "join" tabulek
* implementace extensions používající jmennou konvenci a pluralizaci

## Example 3
* porovnání různých technik pro SQL dotazy
* doplněn Dapper
* doplněn EF
* doplněno ToView pro EF
* doplněno testování výkonu pomocí BenchmarkDotNet

Pro spuštění benchmarku je potřeba být v módu RELEASE. Pro režim DEBUG jsou doplněny "conditional" s výstupem na obrazovku.

Pří seedu je tentokrát naplněno více dat opakovaným spouštěním. Dále je vytvořen na databází pohled/view s názvem **AddressesView**.

## Example 6
* nasazení QueryFiltru z Fullsys Nuget balíčku, rozšíření DB o pole DateCreated v adresách
* nový seed dat
* ukázka funkčních QueryFiltrů v metodě **EfBuildAddress.GetByQuerySamplesFilter**
* vložení nefunkčních/neimplementovaných QueryFiltrů **EfBuildAddress.GetByQuerySamplesFilter**