# Rolling Stock Store

A Railroader mod that adds purchasable gondola cars to the Atlantic Locomotive Works equipment store.

## Features

- Adds 6 different gondola car variants to the in-game store
- Configurable pricing system with customizable base prices
- Price multiplier setting for easy balancing
- Toggle to enable/disable gondola purchases

## Available Gondolas

| Car ID | Name | Base Price |
|--------|------|------------|
| gb-gondola1 | 55ft High-Side Gondola | $1,200 |
| gb-gondola02 | 40ft Wooden Gondola (Steel Frame) | $900 |
| gb-gondola3 | 35ft Wooden Gondola | $800 |
| gb-gondola04 | 70T Mill Gondola | $1,500 |
| gb-gondola05 | Class G30 Gondola | $1,100 |
| gs-gondola06 | G-50-9 Gondola | $950 |

## Installation

1. Install [Unity Mod Manager](https://www.nexusmods.com/site/mods/21)
2. Download the latest release from the [Releases](../../releases) page
3. Extract the zip file to your `Railroader/Mods` folder
4. Launch the game and enable the mod in Unity Mod Manager

## Configuration

Access the mod settings through Unity Mod Manager's mod menu:

- **Enable Gondola Purchases**: Toggle to enable/disable gondola cars in the store
- **Gondola Base Price Multiplier**: Adjust all gondola prices by a multiplier (0.5x - 2.0x)

## Requirements

- Railroader version 2025.1.0b or later
- Unity Mod Manager 0.27.12 or later

## Technical Details

This mod uses Harmony patches to modify the game's equipment purchasing system:

- Patches `EquipmentWindow.ShouldShow()` to make gondolas visible in the store
- Patches `CarDefinition.BasePrice` getter to apply custom pricing
- Uses a two-patch caching architecture to bridge identifier/definition data

## Compatibility

This mod should be compatible with most other Railroader mods. It only modifies the equipment store visibility and pricing for gondola cars.

## Building from Source

### Prerequisites
- .NET Framework 4.8 SDK
- Railroader installed

### Build Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/tacticalcatboy/tc-rollingstockstore.git
   cd tc-rollingstockstore
   ```

2. Set the Railroader install directory (optional, for automatic deployment):
   ```powershell
   $env:RrInstallDir = "C:\Path\To\Railroader"
   ```

3. Build the project:
   ```bash
   dotnet build -c Release
   ```

The compiled DLL will be in `RollingStockStore/bin/Release/net48/`

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Credits

- **Author**: TacticalCatboy (Scarlett)
- Template from [rr-mod-template](https://github.com/mricher-git/rr-mod-template)

## Support

For bugs, feature requests, or questions, please [open an issue](../../issues) on GitHub.
