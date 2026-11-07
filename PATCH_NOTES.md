# حزمة الإصلاحات

انسخ المجلدات فوق جذر المشروع (LegendsStyleRPG/) واستبدل الملفات.

| الملف | الإصلاح |
|---|---|
| GameClient/Packages/manifest.json | يضيف unitywebrequest (يحل أخطاء ApiClient.cs) |
| Backend/app/models.py | DateTime بدون timezone: على PostgreSQL المقارنة كانت تفشل (naive vs aware) وتكسر كل الـ tokens |
| Backend/requirements.txt | إضافة psycopg (بدونه prod.env لا يعمل) |
| Backend/app/config.py | تبسيط منطق DATABASE_URL |
| Backend/app/main.py | lifespan بدل on_event + منع credentials مع CORS "*" |
| Backend/app/api/routes_auth.py | تنظيف التوكنات بـ DELETE واحد |
| Deployment/docker/Dockerfile | مجلد data قابل للكتابة لمستخدم appuser |
| Deployment/docker/docker-compose.yml | يقرأ dev.env (مطابق للتوثيق) |
| Deployment/docker/docker-compose.prod.yml | جديد: PostgreSQL + prod.env |
| Deployment/config/dev.env.example | SQLite داخل الـ volume |
| .gitignore | إصلاح Builds/ و منع رفع *.env الحقيقية |

ملاحظة: ربط Unity Cloud على 2022.3.55f1 يتم من لوحة Unity Cloud وليس من الملفات.
