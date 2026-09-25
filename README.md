# Maskinpark — Learning by Refactoring i Blazor

Lexicon-labb: ett managementsystem för en industriell maskinpark.
Supporttekniker ska kunna **övervaka, administrera och styra** maskiner via ett
dashboard.

Samma idé som MovieApi-Refactor — men för Blazor-komponenter i stället för
API-lager.

## Pedagogisk idé

Vi börjar med det **enklaste som funkar** — en sida med en hårdkodad lista. Sen
refaktorerar vi, men bara när vi känner smärtan av att inte ha refaktorerat.

Varje refaktorering föregås av:

1. **Vad är problemet?** — vi ser vad som är jobbigt just nu
2. **Vilken komponent/vilket lager löser det?** — vi inför ETT i taget
3. **Var bor datan, hur flödar den?** — vi ritar flödet efter varje refaktorering

**Två delar:**

- **Fas 1–8:** Labbens krav (Steg 1–10). Efter Fas 8 är checklistan i Steg 10
  uppfylld.
- **Fas 9–13:** Bonusuppgifterna. Fas 13 byter in-memory mot databas — och
  bevisar att arkitekturen håller.

## Miljö

- .NET 10 (LTS), Blazor Web App
- Interactive render mode: **WebAssembly**, interactivity location: **Global**
- `dotnet` CLI, `gh` CLI, `git`
- LazyVim/Neovim
- Webbläsare + devtools för manuell test
- Kulala.nvim för `.http`-tester (från Fas 13)

## Skapa projektet

Labbinstruktionen visar dialogen i Visual Studio. Motsvarande CLI-kommando:

```bash
dotnet new blazor -n Maskinpark -f net10.0 --interactivity WebAssembly --all-interactive --use-program-main
```

| Inställning i VS | CLI | Kommentar |
|---|---|---|
| Framework: .NET 10.0 | `-f net10.0` | |
| Authentication type: None | — | default |
| Configure for HTTPS ✓ | — | default (`--no-https` stänger av) |
| Interactive render mode: WebAssembly | `--interactivity WebAssembly` | |
| Interactivity location: Global | `--all-interactive` | render mode på toppnivå |
| Include sample pages ✓ | — | default (`--empty` tar bort dem) |
| Do not use top-level statements ✓ | `--use-program-main` | ger `Main`-metod i `Program.cs` |
| .dev.localhost TLD ☐, Aspire ☐ | — | inget att ange |

## Sessionsregler

- **Ett steg åt gången.** Nästa när jag säger "nästa" eller "kör".
- **Ett issue per steg.** Mönster: `Fas N Steg X: <vad>`
- **Verifiering efter varje steg.** `dotnet build` + webbläsaren när UI ändras.
- **Commit efter varje steg.** Mönster: `Fas N Steg X: <vad>. Closes #<issue>`
- **Efter varje refaktorering:** flödesbeskrivning ("var är vi, vem äger datan,
  hur flödar den").

## Faser

### FAS 1 — Skapa projektet 🎯

*Labb Steg 1*

Kör mallen. Starta med `dotnet watch`. Klicka runt i sample-sidorna.

**Mål:** förstå de två projekten — vad körs på servern, vad körs i webbläsaren?

```
Browser → Maskinpark (server, förrenderar HTML) → laddar WASM → Maskinpark.Client tar över
```

### FAS 2 — Layout och navbar

*Labb Steg 3*

**Problem:** Mallens sidomeny passar inte ett dashboard.
**Lösning:** En `NavBar`-komponent högst upp i `MainLayout` — företagsnamn/logga
+ länk till Machine Dashboard.

**Beslut:** behåll eller ta bort sample-sidorna (Counter, Weather)?

### FAS 3 — Datamodell

*Labb Steg 2*

`Machine` i `Models/`: `Id` (Guid), `Name`, `Status`, `LastData`,
`LastUpdated`.

**Beslut:**

- `bool IsOnline` (som labbexemplet) eller `enum MachineStatus`?
- `DateTime` eller `DateTimeOffset`?
- `class` eller `record`?

Inget syns i webbläsaren än — verifiera med `dotnet build`.

### FAS 4 — Enklaste möjliga dashboard

*Labb Steg 4*

En sida. Allt i den. Hårdkodad `List<Machine>` i `@code`. Tabell med
`@foreach`. Statistiken räknas direkt i markup.

**Mål:** maskinerna syns i webbläsaren.

```
Browser → Dashboard.razor → @code { List<Machine> } → HTML-tabell
```

**Beslut:** route `/` (ersätt Home) eller `/dashboard`?

**Lägg märke till:** ladda om sidan — GUID:erna "hoppar". Varför? (Svar i
Fas 8.)

### FAS 5 — Refaktorering: Komponenter

*Labb Steg 5 + 6*

**Problem:** `Dashboard.razor` är en lång fil som blandar tabell, statistik och
data. Svår att läsa, omöjlig att återanvända.
**Lösning:** Bryt ut `MachineStats` och `MachineList` i `Components/`.
Dashboard äger listan och skickar den nedåt via `[Parameter]`.

```
Dashboard (äger List<Machine>)
  ├──[Parameter]──▶ MachineStats   Total · Online · Offline · Senast uppdaterad
  └──[Parameter]──▶ MachineList    Name · Id · Status · Last Data · Actions
```

**Regel:** data flödar **nedåt**. Barnet ändrar aldrig sin parameter.

### FAS 6 — Maskinkontroller

*Labb Steg 7*

**Problem:** Knapparna Start / Stop / Send Data / Delete sitter i
`MachineList` — men listan ägs av `Dashboard`. Hur säger barnet till
föräldern?
**Lösning:** `EventCallback<Machine>` — events flödar **uppåt**.

```
MachineList ──OnStart(machine)──▶ Dashboard → ändrar maskinen → rerender
                                                ├──▶ MachineStats (uppdaterad!)
                                                └──▶ MachineList
```

**Beslut:** vad betyder "Send Data"? (T.ex. ny mätning i `LastData` +
`LastUpdated` = nu.)

**Testa:** starta en maskin — uppdateras statistiken? Varför?

### FAS 7 — Lägg till maskin

*Labb Steg 8*

Knapp `Add Machine` → ny maskin med nytt GUID läggs till i listan.

**Känn efter:** `Dashboard.razor`s `@code` innehåller nu seed-data,
GUID-generering, start, stop, send data, add, delete... Det är affärslogik i en
UI-komponent.

### FAS 8 — Refaktorering: Service-lager

*Labb Steg 9*

**Problem:** Affärslogik och data bor i en sida. Ingen annan sida når
maskinerna. Logiken går inte att testa utan UI.
**Lösning:** `IMachineService` + `InMemoryMachineService` i `Services/`,
registrerad i DI. Dashboard injicerar servicen.

Metoder (labbens namn + `Async`):

| Labben | Vi |
|---|---|
| `GetMachines()` | `GetMachinesAsync()` |
| `AddMachine()` | `AddMachineAsync()` |
| `RemoveMachine()` | `RemoveMachineAsync()` |
| `StartMachine()` | `StartMachineAsync()` |
| `StopMachine()` | `StopMachineAsync()` |
| `UpdateMachineData()` | `UpdateMachineDataAsync()` |

**Varför async när listan ligger i minnet?** I Fas 13 byts implementationen mot
HTTP. Då ska ingen komponent behöva ändras.

```
Dashboard → IMachineService → InMemoryMachineService (List<Machine> i webbläsarens minne)
  ├──[Parameter]──▶ MachineStats
  └──[Parameter]──▶ MachineList ──EventCallback──▶ Dashboard
```

**Förväntad smärta:** förrendering. Servicen måste finnas registrerad där
sidan renderas — annars kraschar den. Och GUID-hoppet från Fas 4 får sin
förklaring.

**Beslut:** `Singleton` eller `Scoped`? Vad betyder de i webbläsaren jämfört med
på servern?

✅ **Efter Fas 8 — labbens Steg 10 uppfyllt:**

- [ ] Visa alla maskiner
- [ ] Starta en maskin
- [ ] Stoppa en maskin
- [ ] Uppdatera maskindata
- [ ] Lägga till en maskin
- [ ] Ta bort en maskin
- [ ] Visa statistik över maskinparken

### FAS 9 — Statusindikatorer (bonus)

**Problem:** Online/offline är bara text. Svårt att skanna av snabbt.
**Lösning:** En liten `StatusBadge`-komponent med färg. CSS isolation
(`StatusBadge.razor.css`) + villkorliga CSS-klasser.

### FAS 10 — Sökfunktion (bonus)

**Problem:** Med många maskiner blir listan svår att överblicka.
**Lösning:** Sökfält med `@bind` + `@bind:event="oninput"`. Den filtrerade
listan **härleds** från originalet — källan ändras inte.

**Beslut:** var bor filtreringen — i `Dashboard` eller i `MachineList`?

### FAS 11 — Redigera maskin (bonus)

**Problem:** En maskin kan bara läggas till och tas bort — inte ändras.
**Lösning:** `EditForm` + `DataAnnotationsValidator` + `InputText`. Ny metod i
servicen: `UpdateMachineAsync()`.

**Beslut:** redigera direkt i `Machine`, eller i en separat formulärmodell?
(Samma fråga som DTOs i MovieApi.)

### FAS 12 — Graf över maskindata (bonus)

**Problem:** `LastData` är bara senaste värdet, och en sträng. Ingen historik,
inga siffror att rita.
**Lösning:** Spara mätvärden som historik per maskin. Rendera som SVG direkt i
Razor — eller via ett diagrambibliotek.

**Beslut:** SVG för hand eller bibliotek?

### FAS 13 — 💥 Databas (bonus)

**Problem:** Ladda om sidan — alla maskiner du lagt till är borta. In-memory i
webbläsaren överlever inte en omladdning.
**Men:** `Maskinpark.Client` körs i webbläsaren. Den kan inte öppna en
SQLite-fil på servern.

**Lösning:**

1. Servern (`Maskinpark`) får EF Core + SQLite + API-endpoints
2. Client får `HttpMachineService : IMachineService` som anropar API:et via
   `HttpClient`
3. Byt registreringen i DI — **ingen komponent ändras**

```
Dashboard → IMachineService → HttpMachineService ──HTTP──▶ API (server) → DbContext → SQLite
```

**Beslut:**

- Controllers (som MovieApi) eller minimal API?
- Var ligger `Machine` så att båda projekten ser den? (Servern refererar redan
  Client.)
- Förrenderingen sker på servern — vilken implementation av `IMachineService`
  ska servern använda?

**När du byter en registrering i `Program.cs` och hela appen fungerar mot
databasen — då förstår du varför interfacet infördes i Fas 8.**

## Labbsteg → Fas

| Labbsteg | Fas |
|---|---|
| 1 — Skapa projektet | 1 |
| 2 — Datamodell | 3 |
| 3 — Layout och navigation | 2 |
| 4 — Dashboard-sida | 4 |
| 5 — Statistikkomponent | 5 |
| 6 — Maskinlista | 5 |
| 7 — Maskinkontroller | 6 |
| 8 — Lägg till maskin | 7 |
| 9 — Backend / Datahantering | 8 |
| 10 — Funktionalitet som ska fungera | ✅ efter 8 |
| Bonus: Statusindikatorer med färger | 9 |
| Bonus: Sökfunktion | 10 |
| Bonus: Redigera maskin | 11 |
| Bonus: Graf över maskindata | 12 |
| Bonus: Spara data i databas | 13 |

## Slutarkitektur (efter Fas 13)

```
Maskinpark.Client  (körs i webbläsaren)
  Pages/Dashboard.razor
    ├─ Components/MachineStats.razor
    └─ Components/MachineList.razor
         └─ Components/StatusBadge.razor
  Services/IMachineService
    └─ HttpMachineService ──HTTP──┐
  Models/Machine                  │
                                  ▼
Maskinpark  (körs på servern)
  API-endpoints
    └─ DbContext (EF Core) → SQLite
```

**Dataflöde i en mening:** sidan hämtar via servicen, data går nedåt som
parametrar, events går uppåt som callbacks, och bara servicen vet var datan
faktiskt bor.

## Vad jag lär mig efter varje fas

| Efter fas | Jag förstår |
|---|---|
| 1 | Blazor Web App = två projekt: servern förrenderar, WASM tar över i webbläsaren |
| 2 | Layout = ram runt alla sidor (`@Body`), `NavLink` vet vilken sida som är aktiv |
| 3 | Modell = ren data, inga beroenden |
| 4 | Razor = HTML + C# i samma fil (`@foreach`, `@code`) |
| 5 | Komponenter + `[Parameter]` = data flödar nedåt |
| 6 | `EventCallback` = events flödar uppåt, föräldern äger state |
| 7 | UI-komponenter fylls snabbt med affärslogik om inget stoppar det |
| 8 | Service + DI = logik skild från UI, interface = utbytbar implementation |
| 9 | CSS isolation = stil per komponent, små återanvändbara komponenter |
| 10 | Härledd data — filtrera utan att ändra källan |
| 11 | `EditForm` + validering |
| 12 | Data måste ha rätt form för att kunna visualiseras |
| 13 | WASM körs i webbläsaren → databas kräver API. Interfacet betalar sig |

## Vad detta INTE innehåller (medvetet)

- ❌ Autentisering (labben: *None*)
- ❌ Interactive Server / Auto render mode — bara WebAssembly
- ❌ Realtid (SignalR) — maskiner som själva pushar data (bra fortsättning)
- ❌ Komponentbibliotek (MudBlazor m.fl.) — vi bygger själva
- ❌ Aspire, deploy

Fokus här: **komponenter, dataflöde och lager — från en sida till en app med
utbytbar backend**.

## Referenser

- Labbinstruktion: `Blazor_Maskinpark.pdf` (Lexicon)
- [Default templates for `dotnet new`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new-sdk-templates) — `blazor`-mallens flaggor
- [ASP.NET Core Blazor project structure](https://learn.microsoft.com/en-us/aspnet/core/blazor/project-structure?view=aspnetcore-10.0)
- [Tooling for ASP.NET Core Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/tooling?view=aspnetcore-10.0)
- Systerövning: MovieApi-Refactor
