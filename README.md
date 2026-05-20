# SmartFocus

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![Windows](https://img.shields.io/badge/Windows-10%2B-0078D6?style=flat&logo=windows)
![Licencia](https://img.shields.io/badge/licencia-MIT-00FFFF?style=flat)

**SmartFocus** es un lanzador ultraligero para Windows que te permite **buscar y traer al frente cualquier ventana abierta** en un instante.  
Inspirado en Spotlight, PowerToys Run y Alfred, pero centrado exclusivamente en control de ventanas con una estética **cyberpunk elegante**.

---

## Características principales

- 🔍 **Búsqueda difusa tolerante a errores** – escribe “disc” y obtén Discord, “vs” para Visual Studio Code.
- ⚡ **Enfoque instantáneo** – restaura ventanas minimizadas, las trae al frente y les da foco real (evita el *focus stealing* de Windows).
- 🧠 **Aprendizaje por frecuencia** – reordena los resultados según tus hábitos de uso.
- 🏷️ **Alias personalizables** – define `navegador → Firefox`, `música → Spotify`.
- ⌨️ **Hotkey global configurable** – por defecto `Windows+Y`.
- 🎨 **UI minimalista con glow cian** – ventana flotante, fondo oscuro, tipografía monoespaciada.
- 📦 **Bajo consumo de recursos** – ocupa menos de 50 MB de RAM en reposo.
- 🔧 **Arquitectura modular** – lista para extensiones futuras (comandos, pestañas, Everything Search).

---

## Capturas de pantalla

<p align="center">
  <img src="docs/screenshot.png" alt="SmartFocus en acción" width="600"/>
  <br/>
  <sup><i>Barra de búsqueda flotante con resultado resaltado.</i></sup>
</p>

---

## Instalación

### Descarga directa
Ve a [Releases](../../releases) y descarga el archivo `SmartFocus_Setup.exe` (instalador MSIX) o la versión portátil `SmartFocus.zip`.

### Desde el código fuente
Requisitos:
- [.NET 8 SDK](https://dotnet.microsoft.com/es-es/download/dotnet/8.0)
- Windows 10 o superior
- Visual Studio 2022 (opcional, puedes usar `dotnet build`)

```bash
git clone https://github.com/tuusuario/SmartFocus.git
cd SmartFocus
dotnet build -c Release
dotnet run --project src/SmartFocus.UI
