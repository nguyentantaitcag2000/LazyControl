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
- In cases where text input is needed, disable the mouse feature to type using the set shortcut, default is `Ctrl` + `J`
### Focus on the Active Window on a Screen
- Suppose you have multiple applications open on your computer and are using two monitors—one for coding and one for browsing. While coding, you may want to switch to the other monitor to search or perform actions. Instead of reaching for the mouse to focus on the browser, you can set a shortcut to quickly focus. For example, set `ESC` + `F1` for the left monitor and `ESC` + `F2` for the right monitor. This way, switching between monitors is as simple as pressing `ESC` + `F1` or `ESC` + `F2`.
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

---

### Di chuyển cursor
- Qua trái:   `A`
- Qua phải:   `D`
- Lên trên:   `W`
- Xuống dưới: `S`

- Di chuyển góc trái bên trên: `A W`
- Di chuyển góc trái bên dưới: `A S`
- Di chuyển góc phải bên trên: `W D`
- Di chuyển góc phải bên dưới: `S D`

### Scroll / Cuộn
- Qua trái:   Nhấn đồng thời `L` + `W`
- Qua phải:   Nhấn đồng thời `L` + `S`
- Lên trên:   Nhấn đồng thời `L` + `W`
- Xuống dưới: Nhấn đồng thời `L` + `S`

### Click chuột
- Trái: `J`
- Phải: `K`
- Giữa: `N` -> Thường sử dụng để tắt nhanh các tab trình duyệt hoặc các tab ở các phần mềm khác
### Select text 
- Y như hành vi dùng chuột, ở 1 điểm nhấn nghiến `J` sau đó di chuyển cursor `A`/`W`/`D`/`S`

### Tắt chế độ di chuyển chuột bằng phím
- Giả sử trong trường hợp cần nhập liệu văn bản, ta sẽ tắt tính năng chuột này đi để đánh văn bản bằng phím tắt đã thiết lập, mặc định là `Ctrl` + `J`

### Focus vào cửa sổ đang hiển thị ở 1 màn hình
- Giả sử bạn đang có cả chục ứng dụng đang mở trên máy, bạn sử dụng 2 màn hình, bạn đang mở 1 nàn hình để viết code, 1 màn hình để hiển thị trang web, trong lúc viết code, bạn muốn chuyển sang màn hình bên kia để tìm kiếm hoặc thao tác gì đó, bạn sẽ đưa tay ra lấy chuột chỉ để click focus vào trình duyệt trên màn hình đó để tìm kiếm, thao tác,... để tránh sự gườm rà này, tôi đã thiết lập 1 phím tắt để focus nhanh chóng, giả sử tôi set `ESC` + `F1` là màn hình bên trái, `ESC` + `F2` là màn hình bên phải, như vậy, mỗi lần tôi muốn chuyển qua lại giữa 2 màn hình chỉ cần nhấn `ESC` + `F1` hoặc `ESC` + `F2`

### Tăng/ giảm âm lượng
- Giảm âm lượng: `Ctrl` + `F7`
- Tăng âm lượng: `Ctrl` + `F8`

Nếu trong trường hợp bạn nhấn nó focus ngược màn hình, thì cứ vào Setting để điều chỉnh lại nhé

### Khác
- Phần mềm này mình thiết kế các phím tắt nằm ở vị trí rất tự nhiên, dường như các phím tắt đều nằm ở các vị trí mà tất cả chúng ta đều đặt tay lên mặc định.
- Bên phím di chuyển `A`/`W`/`D`/`S` rất quen thuộc với ai chơi game, các nút này cũng thường là phím tắt di chuyển trong game
- Chế độ chuột sẽ tự tắt đi khi mà bạn sử dụng các nút `Ctrl` và `Window` - Giúp bạn có thể sử dụng các nút chức năng của 1 máy tính cơ bản như `Ctrl` + `C` và `Ctrl` + `V`, `Window` + `Shift` + `S`,... mà không cần tắt đi chế độ di chuyển chuột của **LazyControl**.
- Khi chế độ di chuyển chuột bật lên, bạn sẽ thấy 1 vòng tròn được highlight lên mờ mờ giúp bạn có thể nhận biết được mà có thể bật/tắt cho phù hợp


## Cách deploy
- Tăng số version ở file Configuration.cs `public const string VERSION = "1.0.0.13";`
- Chạy file `build-single.bat` để tạo ứng dụng portable
- Sau đó nó tạo ra thư mục `my-publish`, có chứa 2 file `.exe` và `.xml`, upload 2 file này lên server