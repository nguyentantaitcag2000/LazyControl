# LazyControl

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
- Giả sử trong trường hợp cần nhập liệu văn bản, ta sẽ tắt tính năng chuột này đi để đánh văn bản bằng phím tắt đã thiết lập, mặc định là `Alt` + `J`

### Focus vào cửa sổ đang hiển thị ở 1 màn hình
- Giả sử bạn đang có cả chục ứng dụng đang mở trên máy, bạn sử dụng 2 màn hình, bạn đang mở 1 nàn hình để viết code, 1 màn hình để hiển thị trang web, trong lúc viết code, bạn muốn chuyển sang màn hình bên kia để tìm kiếm hoặc thao tác gì đó, bạn sẽ đưa tay ra lấy chuột chỉ để click focus vào trình duyệt trên màn hình đó để tìm kiếm, thao tác,... để tránh sự gườm rà này, tôi đã thiết lập 1 phím tắt để focus nhanh chóng, giả sử tôi set `ESC` + `F1` là màn hình bên trái, `ESC` + `F2` là màn hình bên phải, như vậy, mỗi lần tôi muốn chuyển qua lại giữa 2 màn hình chỉ cần nhấn `ESC` + `F1` hoặc `ESC` + `F2`

### Tăng/ giảm âm lượng
- Giảm âm lượng: `ESC` + `F7`
- Tăng âm lượng: `ESC` + `F8`

Nếu trong trường hợp bạn nhấn nó focus ngược màn hình, thì cứ vào Setting để điều chỉnh lại nhé

### Khác
- Phần mềm này mình thiết kế các phím tắt nằm ở vị trí rất tự nhiên, dường như các phím tắt đều nằm ở các vị trí mà tất cả chúng ta đều đặt tay lên mặc định.
- Bên phím di chuyển `A`/`W`/`D`/`S` rất quen thuộc với ai chơi game, các nút này cũng thường là phím tắt di chuyển trong game
- Chế độ chuột sẽ tự tắt đi khi mà bạn sử dụng các nút `Ctrl` và `Window` - Giúp bạn có thể sử dụng các nút chức năng của 1 máy tính cơ bản như `Ctrl` + `C` và `Ctrl` + `V`, `Window` + `Shift` + `S`,... mà không cần tắt đi chế độ di chuyển chuột của **LazyControl**.
- Khi chế độ di chuyển chuột bật lên, bạn sẽ thấy 1 vòng tròn được highlight lên mờ mờ giúp bạn có thể nhận biết được mà có thể bật/tắt cho phù hợp


## Cách deploy
- Tăng số version ở file Configuration.cs `public const string VERSION = "1.0.0.13";`
- Chạy file build-single.bat để tạo ứng dụng portable
- Sau đó nó tạo ra thư mục `my-publish`, có chứa 2 file `.exe` và `.xml`, upload 2 file này lên R2