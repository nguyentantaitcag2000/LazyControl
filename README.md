# LazyControl

### Cursor Movement
- Left: `A`
- Right: `D`
- Up: `W`
- Down: `S`
- Move to top-left corner: `A W`
- Move to bottom-left corner: `A S`
- Move to top-right corner: `W D`
- Move to bottom-right corner: `S D`

### Scroll
- Scroll left: Press `L` + `W` simultaneously
- Scroll right: Press `L` + `S` simultaneously
- Scroll up: Press `L` + `W` simultaneously
- Scroll down: Press `L` + `S` simultaneously

### Mouse Click
- Left click: `J`
- Right click: `K`
- Middle click: `N` -> Often used to quickly close browser tabs or tabs in other software

### Select Text
- Similar to mouse behavior: at a point, hold `J` then move the cursor with `A`/`W`/`D`/`S`

### Disable Mouse Movement Mode
- In cases where text input is needed, disable the mouse feature to type using the set shortcut, which can be chosen flexibly between `Ctrl` + `J`, `Ctrl` + `K`, or `Ctrl` + `L`. Default is `Ctrl` + `J`

### Focus on the Active Window on a Screen
- Suppose you have multiple applications open on your computer and are using two monitors—one for coding and one for browsing. While coding, you may want to switch to the other monitor to search or perform actions. Instead of reaching for the mouse to focus on the browser, you can set a shortcut to quickly focus. For example, set `ESC` + `F1` for the left monitor and `ESC` + `F2` for the right monitor. This way, switching between monitors is as simple as pressing `ESC` + `F1` or `ESC` + `F2`.

### Move Caret/Cursor to Center of Another Monitor
- When using two monitors and mouse movement mode is enabled, pressing `F1` or `F2` (without `ESC`) will move the caret/cursor directly to the center of the corresponding monitor, regardless of its current position. This allows for quick caret/cursor relocation between screens.

### Increase/Decrease Volume
- Decrease volume: `Ctrl` + `F7`
- Increase volume: `Ctrl` + `F8`
If pressing these focuses the wrong monitor, adjust it in the Settings.

### Others
- This software is designed with shortcuts placed in natural positions, as if they align with where our hands naturally rest.
- The movement keys `A`/`W`/`D`/`S` are familiar to gamers, as these are commonly used for movement in games.
- Mouse mode automatically disables when using `Ctrl` or `Windows` keys, allowing you to use basic computer functions like `Ctrl` + `C`, `Ctrl` + `V`, `Windows` + `Shift` + `S`, etc., without disabling **LazyControl**'s mouse movement mode.
- When mouse movement mode is active, a faintly highlighted circle appears to indicate it’s enabled, helping you toggle it on/off as needed.

## How to Deploy
- Increment the version number in the `Configuration.cs` file: `public const string VERSION = "1.0.0.13";`
- Run the `build-single.bat` file to create a portable application
- This generates a `my-publish` folder containing two files: `.exe` and `.xml`. Upload these two files to the server.
- Open .xml file and add the url inside <changelog> for users to see the changelog when they update.