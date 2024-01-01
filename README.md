
# Windows My Paint

C# - WPF  Application

Simple drawing desktop app



## Thành viên

1. Trương Minh Phát - 21120524

2. Nguyễn Phúc Phát - 21120521

3. Trần Đức Minh - 21120502




## A. Mô tả

  Chương trình vẽ đồ họa dựa trên tư duy đơn giản hóa phần mềm MS Paint của hệ điều hành Windows


## Các chức năng cơ sở (dựa trên yêu cầu đồ án)


- [x] Dynamically load all graphic objects that can be drawn from external DLL files

- [x] The user can choose which object to draw

- [x] The user can see the preview of the object they want to draw

- [x] The user can finish the drawing preview and their change becomes permanent with previously drawn objects

- [x] The list of drawn objects can be saved and loaded again for continuing later (json)

- [x] Save and load all drawn objects as an image in bmp/png/jpg format (rasterization). Just one format is fine. No need to save in all three formats.



## Các dạng hình học cơ bản có hổ trợ
1. Line
2. Rectangle
3. Ellipse
4. Circle
5. Square

## Các cải tiến dựa trên yêu cầu đồ án

- [x] Allow the user to change the color, pen width, stroke type (dash, dot, dash dot dot...)

- [x] Adding image to the canvas

- [x] Select a single element for editing again

- [x] Drag & Drop

- [x] Undo, Redo (Command)

- [x] Reduce flickering when drawing preview by using buffer to redraw all the canvas

- [x] Upgrade: Only redraw the needed region, no fullscreen redraw

- [x] Copy / Paste / Cut (command)

- [x] Rotate the shape

- [x] Transforming horizontally and vertically
