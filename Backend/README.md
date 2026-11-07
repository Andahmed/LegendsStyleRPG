# Backend — LegendsStyleRPG API

خدمة الـ Backend للعبة (Python / FastAPI). في المرحلة 0.1 يعمل كخدمة مرجعية مُختبرة؛ الربط الفعلي من داخل اللعبة يبدأ في المراحل اللاحقة عبر `ApiClient` مع إمكانية تغيير عنوان الـ API دون تعديل منطق اللعبة.

## التشغيل السريع (بيئة التطوير)

```bash
cd Backend
python3 -m venv .venv
source .venv/bin/activate          # Windows: .venv\Scripts\activate
pip install -r requirements.txt

# تشغيل السيرفر
uvicorn app.main:app --reload --port 8000

# التحقق
curl http://localhost:8000/api/v1/health
```

فتح توثيق API التفاعلي (بيئة التطوير فقط): `http://localhost:8000/docs`

## الاختبارات

```bash
python -m pytest tests/ -v
```

تغطي: الصحة، التسجيل، الدخول، كلمة المرور الخاطئة، تكرار اسم المستخدم، حفظ واسترجاع البيانات، حماية Anti-Rollback، ورفض الطلبات غير الموثقة.

## نقاط النهاية (v1)

| Method | Path | الوصف |
|--------|------|-------|
| GET | `/api/v1/health` | فحص جاهزية الخدمة |
| POST | `/api/v1/auth/register` | إنشاء حساب |
| POST | `/api/v1/auth/login` | تسجيل دخول → Bearer token |
| POST | `/api/v1/auth/logout` | إبطال التوكن |
| GET | `/api/v1/auth/me` | بروفايل الحساب الحالي |
| GET | `/api/v1/player/profile` | بروفايل اللاعب |
| PUT | `/api/v1/player/save` | كتابة حفظ سحابي (مع حماية Anti-Rollback) |
| GET | `/api/v1/player/save?slot=0` | قراءة حفظ سحابي |

## قواعد أمنية مطبقة

- كلمات المرور: PBKDF2-HMAC-SHA256 (240k تكرار) مع Salt لكل مستخدم.
- التوكنات: قيم عشوائية 256-bit مع انتهاء صلاحية وإمكانية الإبطال.
- السيرفر هو المرجع: مستوى اللاعب في الحفظ لا يقبل التراجع (409).
- لا توجد أسرار داخل الكود: كل الإعدادات من متغيرات البيئة.
- رسائل خطأ موحدة عند فشل الدخول (لا تفضح ما إذا كان الخطأ في الاسم أم كلمة المرور).

## الانتقال إلى PostgreSQL

اضبط `DATABASE_URL` فقط، مثال:
`DATABASE_URL=postgresql+psycopg://user:pass@host:5432/legends`

## الترحيلات (Migrations)

للتطوير: `init_db()` ينشئ الجداول تلقائيًا. للإنتاج يوصى بإضافة Alembic:
`alembic init migrations` ثم توليد أول migration من `app.models.Base.metadata`.
