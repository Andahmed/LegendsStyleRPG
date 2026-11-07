# Server Setup — تشغيل سيرفر اللعبة

> ملاحظة مهمة: لم نفترض نظام تشغيل سيرفرك ولا مواصفاته. الخطوات أدناه صالحة على أي نظام يدعم Python 3.11+ أو Docker. تأكد من المنافذ المفتوحة في جدار الحماية عندك قبل اختبار الاتصال من شبكة خارجية.

## 1. متطلبات التشغيل

| البند | الحد الأدنى | ملاحظات |
|-------|-------------|---------|
| Python | 3.11+ | أو Docker فقط |
| RAM | 512MB | لمرحلة النموذج الأولي |
| منافذ | 8000 (API) | قابلة للتغيير عبر `PORT` |
| قاعدة بيانات | SQLite (تطوير) / PostgreSQL 16 (إنتاج) | عبر `DATABASE_URL` |

## 2. التشغيل المباشر (بدون Docker)

```bash
cd Backend
python3 -m venv .venv && source .venv/bin/activate
pip install -r requirements.txt
uvicorn app.main:app --host 0.0.0.0 --port 8000 --workers 2
```

## 3. التشغيل عبر Docker

```bash
cd Deployment/docker
cp ../config/dev.env.example ../config/dev.env   # عدّل إن لزم
docker compose up -d --build
docker compose logs -f api        # متابعة السجلات
```

## 4. أوامر التشغيل/الإيقاف/إعادة التشغيل

```bash
# Docker
docker compose up -d            # تشغيل
docker compose stop             # إيقاف
docker compose restart api      # إعادة تشغيل
docker compose down             # إيقاف وإزالة الحاويات

# بدون Docker (systemd example)
sudo systemctl start legends-api
sudo systemctl stop legends-api
sudo systemctl restart legends-api
```

## 5. النسخ الاحتياطي لقاعدة البيانات

- SQLite (تطوير): انسخ ملف `legends_dev.db` بعد إيقاف الكتابة:
  `sqlite3 legends_dev.db ".backup 'backup_$(date +%F).db'"`
- PostgreSQL (إنتاج):
  `pg_dump -U legends legends > backup_$(date +%F).sql`
- جدولة النسخ الاحتياطي: cron يومي 03:00 مع الاحتفاظ 14 يومًا (مثال أدناه):

```cron
0 3 * * * cd /srv/backend && ./scripts/backup_db.sh
```

## 6. اختبار الاتصال من خارج السيرفر (إلزامي قبل التسليم)

```bash
# من جهاز خارج شبكة السيرفر تمامًا:
curl -i https://YOUR_SERVER/api/v1/health     # أو http://IP:8000/api/v1/health
```

النتيجة المتوقعة: `{"status":"ok","env":"..."}`. إن فشل: تحقق من جدار الحماية، منفذ الـ reverse proxy، وربط النطاق.

## 7. فصل الإعدادات

- التطوير: `Deployment/config/dev.env` — SQLite + CORS مفتوح + docs مفعلة.
- الإنتاج: `Deployment/config/prod.env` — PostgreSQL + CORS مقصور + docs معطلة + توكن أقصر عمرًا.
- **عنوان الـ API في تطبيق الهاتف** يُضبط من مكان واحد داخل كود العميل (`ApiClient`) وقابل للتغيير دون إعادة كتابة منطق اللعبة.
- يُمنع وضع كلمات مرور أو مفاتيح داخل ملفات اللعبة أو الـ APK.

## 8. متطلبات النشر النهائي (قائمة تحقق)

- [ ] `curl` من شبكة خارجية ينجح
- [ ] سجلات الأخطاء خالية من أخطاء حرجة (`docker compose logs api`)
- [ ] نسخة احتياطية تجريبية أُنشيت واستُعيدت بنجاح
- [ ] الإعدادات الحساسة في env وليست في الكود
- [ ] سيرفر يعيد التشغيل تلقائيًا (`restart: unless-stopped` / systemd)
