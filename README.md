
# SmartFocus

[![.NET](https://img.shields.io/badge/.NET-8.0%2B-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![Windows](https://img.shields.io/badge/Windows-11-0078D6?style=flat&logo=windows)](https://www.microsoft.com/windows/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Status](https://img.shields.io/badge/Status-In%20Development-orange)]()

> Un lanzador ultraligero para Windows que te permite **buscar y enfocarse en cualquier ventana abierta al instante**.  
> Inspirado en Spotlight, PowerToys Run y Alfred, pero especializado exclusivamente en **control inteligente de ventanas** con una estética **cyberpunk elegante**.

---

## ✨ Características

- 🔍 **Búsqueda difusa tolerante a errores** – Escribe "disc" y obtén Discord, "vs" para Visual Studio
- ⚡ **Enfoque instantáneo** – Restaura, trae al frente y enfoca ventanas sin robo de foco
- 🧠 **Aprendizaje automático** – Reordena resultados según tus hábitos de uso
- 🏷️ **Alias personalizables** – Define atajos: `navegador → Firefox`, `música → Spotify`
- ⌨️ **Hotkey global** – Abre con `Windows+Y` (configurable)
- 🎨 **UI minimalista** – Ventana flotante, tema oscuro, tipografía monoespaciada con glow cian
- 📦 **Bajo consumo** – Solo 35-40 MB de RAM en reposo
- 🔧 **Arquitectura modular** – Preparada para extensiones futuras (comandos, pestañas, etc.)

---

## 📸 Vista Previa

<p align="center">
  <img src="docs/screenshot.png" alt="SmartFocus buscando Discord" width="500"/>
  <br/>
  <sub><i>Búsqueda difusa: "dis" → Discord</i></sub>
</p>

---

## 🚀 Inicio Rápido

### Desde Código Fuente

**Requisitos:**
- [.NET 8 SDK](https://dotnet.microsoft.com/es-es/download/dotnet/8.0)
- Windows 11
- Visual Studio 2022 (opcional)

**Instalación:**
```bash
git clone https://github.com/YacaloX/SmartFocus.git
cd SmartFocus
dotnet build
dotnet run
```

### Descarga Directa

> ⚠️ **Los releases estables estarán disponibles próximamente**. De momento, compila desde el código fuente.

---

## 🏗️ Arquitectura

SmartFocus está estructurado en **módulos independientes** para facilitar mantenimiento y extensiones:

```
SmartFocus/
├── Core/                      # Lógica principal
│   ├── WindowManager.cs       # Detección y enfoque de ventanas
│   ├── SearchEngine.cs        # Búsqueda difusa
│   ├── HistoryTracker.cs      # Aprendizaje por frecuencia
│   ├── AliasManager.cs        # Gestión de alias personalizados
│   ├── HotkeyService.cs       # Hotkey global (Windows+Y)
│   ├── AppPaths.cs            # Rutas de aplicación
│   └── Models/
│       ├── WindowInfo.cs      # Información de ventana
│       ├── HistoryEntry.cs    # Registro de frecuencia/última vez usado
│       └── MainViewModel.cs   # MVVM ViewModel
├── UI/                        # Interfaz WPF
│   ├── MainWindow.xaml        # Layout
│   └── MainWindow.xaml.cs     # Code-behind
├── App.xaml                   # Configuración de aplicación
└── SmartFocus.csproj          # Proyecto
```

### Módulos Principales

| Módulo | Responsabilidad |
|--------|-----------------|
| **WindowManager** | Enumera ventanas abiertas, las trae al frente y resuelve problemas de focus stealing |
| **SearchEngine** | Búsqueda fuzzy en títulos de ventanas y procesos |
| **HistoryTracker** | Almacena uso frecuencia → reordena resultados inteligentemente |
| **AliasManager** | Permite crear atajos personalizados para ventanas |
| **HotkeyService** | Captura hotkey global sin interferir con otras aplicaciones |

---

## ⚙️ Configuración

### Alias Personalizados

Edita `%AppData%/SmartFocus/aliases.json`:

```json
{
  "navegador": "firefox",
  "música": "spotify",
  "código": "visual studio code",
  "chat": "discord"
}
```

Ahora buscando "nav" abrirá Firefox.

### Cambiar Hotkey

La configuración del hotkey está en `HotkeyService.cs`. Edita la constante:

```csharp
private const Keys HOTKEY = Keys.Y;  // Ctrl+Y
```

- Por el momento **no hay manera de modificar la hotkey en la versión precompilada,** puedes intentar con Cheat Engine o compilando el código tu mismo

---

## 📊 Uso

1. **Presiona `Windows+Y`** – Abre la barra de búsqueda
2. **Escribe lo que buscas** – Escribe el nombre de la app (soporta typos)
3. **Selecciona con ↑↓ o ratón** – Navega resultados
4. **Presiona `Enter`** – Enfoca la ventana

**Ejemplo:**
```
Búsqueda: "vs"
↓ Resultados:
  1. visual studio code (C:\Users\...\Code.exe)
  2. Visual Studio 2022 (devenv.exe)
```

---

## 🔄 Cómo Funciona el Aprendizaje

SmartFocus registra cada ventana que enfocas:

- **Frecuencia**: ¿Cuántas veces has abierto Discord?
- **Última vez**: ¿Cuándo lo abriste por última vez?

Los resultados se reordenan automáticamente basado en tus hábitos. Si siempre abres Chrome antes de Firefox, Chrome aparecerá primero en "bro".

**Archivo**: `%AppData%/SmartFocus/history.json`

---

## 🛠️ Desarrollo

### Estructura de Clases

```csharp
// Información de una ventana
public record WindowInfo(
    IntPtr Handle,
    string Title,
    uint ProcessId,
    string? ProcessName
);

// Resultado de búsqueda
public class SearchResult
{
    public string DisplayName { get; set; }
    public string WindowTitle { get; set; }
    public IntPtr Handle { get; set; }
    public string ProcessName { get; set; }
}
```

### Extensiones Futuras (Roadmap)

- [ ] Soporte para comandos de sistema (abrir archivos, ejecutar scripts)
- [ ] Pestañas dinámicas (Windows, Aplicaciones, Archivos, etc.)
- [ ] Integración con Everything Search (búsqueda de archivos)
- [ ] Temas personalizables
- [ ] Plugin system
- [ ] Tests unitarios
- [ ] Modificación de la interfaz
- [ ] Modificación de la Hotkey

---

## 🐛 Problemas Conocidos

- Focus stealing en algunas aplicaciones (Chromium) – En investigación
- Rendimiento con 100+ ventanas abiertas – Optimización pendiente

**¿Encontraste un bug?** [Abre una issue](https://github.com/YacaloX/SmartFocus/issues)

---

## 💡 Contribuciones

¡Las contribuciones son bienvenidas! Por favor:

1. Fork el repositorio
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit cambios (`git commit -m 'Add AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

---

## 📄 Licencia

Este proyecto está bajo la licencia **MIT** – Ver [LICENSE](LICENSE) para detalles.

---

## 🙋 Soporte

¿Preguntas o sugerencias?

- 📬 [Abre una Discussion](https://github.com/YacaloX/SmartFocus/discussions)
- 🐛 [Reporta un bug](https://github.com/YacaloX/SmartFocus/issues)
- ⭐ ¡Dale una estrella si te gusta el proyecto!

---

## 🎨 Inspiración

- **Spotlight** (macOS) – Búsqueda rápida
- **PowerToys Run** (Windows) – Disponibilidad global
- **Alfred** (macOS) – Aprendizaje automático

---

## 📈 Estadísticas

![GitHub stars](https://img.shields.io/github/stars/YacaloX/SmartFocus?style=social)
![GitHub watchers](https://img.shields.io/github/watchers/YacaloX/SmartFocus?style=social)

---

**Hecho con ❤️ en C# + WPF**
```
