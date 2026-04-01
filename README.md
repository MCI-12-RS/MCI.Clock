# MCI Clock

Relógio e Cronômetro Regressivo para exibição em palco.

Aplicação desktop construída com **.NET 9** e **Avalonia UI**, projetada para exibir um relógio ou cronômetro regressivo em tela cheia, ideal para uso em eventos e apresentações com segundo monitor.

## Funcionalidades

- **Relógio** — Exibe a hora atual em fonte gigante, visível à distância
- **Cronômetro Regressivo** — Contagem regressiva configurável com alerta visual nos últimos minutos
- **Segundo Monitor** — Janela de exibição abre preferencialmente no segundo monitor
- **Alerta Visual** — Quando o cronômetro atinge os últimos 3 minutos (configurável), o display pisca suavemente com fade in/out em vermelho
- **Interface em Português (BR)**

## Requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

## Como Executar

```bash
dotnet run
```

## Como Compilar (Release)

**Windows:**
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o bin/Publish
```

**macOS (Apple Silicon):**
```bash
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o bin/Publish
```

## Release Automático

Ao criar uma tag no formato `v*` (ex: `v1.0.0`), o GitHub Actions gera automaticamente executáveis para:
- **Windows** (x64) — `MciClock.exe`
- **macOS** (Apple Silicon) — `MciClock`

## Licença

[MIT](LICENSE)
