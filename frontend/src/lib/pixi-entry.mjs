// src/lib/pixi-entry.mjs — Tree-shaken PixiJS entry (B3).
// Tránh import root 'pixi.js' kéo toàn bộ thư viện; chỉ deep-import các init/module
// cần thiết qua relative path (exports map của pixi.js chặn deep import /lib/* bằng
// specifier, nên dùng đường dẫn tương đối). Vite alias `pixi.js` -> file này để
// 6 file TypeScript import 'pixi.js' vẫn hoạt động mà không phải sửa chúng.
import '../../node_modules/pixi.js/lib/environment-browser/browserAll.mjs';
import '../../node_modules/pixi.js/lib/app/init.mjs';
import '../../node_modules/pixi.js/lib/scene/graphics/init.mjs';
import '../../node_modules/pixi.js/lib/scene/text/init.mjs';
export { Application } from '../../node_modules/pixi.js/lib/app/Application.mjs';
export { Container } from '../../node_modules/pixi.js/lib/scene/container/Container.mjs';
export { Graphics } from '../../node_modules/pixi.js/lib/scene/graphics/shared/Graphics.mjs';
export { Text } from '../../node_modules/pixi.js/lib/scene/text/Text.mjs';
export { TextStyle } from '../../node_modules/pixi.js/lib/scene/text/TextStyle.mjs';
