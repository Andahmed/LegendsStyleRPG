# إضافة رسومات الواجهة من الموبايل (بدون Unity)

1. من المتصفح افتح **opengameart.org/content/ui-pack-rpg-extension** ونزّل `UIpack_RPG.zip` (ترخيص CC0، حر).
2. فك الضغط في مدير الملفات (Files).
3. افتح **github.com** في المتصفح (وضع Desktop site) > الريبو بتاعك > مسار `GameClient/Assets/Resources/UISkin`.
4. **Add file > Upload files** واختار ملفات PNG (لحد 100 ملف في المرة) > **Commit changes**.
5. شغّل Build جديد من Unity Cloud. الصور بتتحوّل لـ Sprites تلقائياً (`Editor/UISkinImporter.cs`) والقوائم بتستخدمها.

- مفيش صورة؟ اللعبة بترجع للشكل المرسوم بالكود.
- عشان حد معين: سمّي الملف `button.png` أو `panel.png` أو `panel_inset.png`، أو سيب أسماء Kenney زي ما هي.
- لتغيير حواف التمدد (9-slice): ضيف `_b16` في آخر الاسم.
