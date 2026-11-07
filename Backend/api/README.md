# api/ — نقاط النهاية (Routers)

نقاط النهاية الفعلية تعيش داخل حزمة التطبيق في `app/api/` (بنية Python القياسية):
- `app/api/routes_health.py` — فحص الجاهزية
- `app/api/routes_auth.py` — التسجيل والدخول والخروج و«حسابي»
- `app/api/routes_player.py` — الحفظ السحابي لللاعب (مع Anti-Rollback)

أي مسار جديد يُضاف كملف router في `app/api/` ثم يُسجَّل في `app/main.py`.
هذا المجلد موجود للتوثيق والفصل وفق هيكل التكليف.
