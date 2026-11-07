# database/ — قاعدة البيانات

- التطوير: SQLite (`legends_dev.db` تُنشأ تلقائيًا عند أول تشغيل).
- الإنتاج: PostgreSQL عبر `DATABASE_URL` (انظر `../Documentation/ServerSetup.md`).
- تعريف الجداول: `app/models.py` (players / auth_tokens / player_saves).
- الاتصال والجلسات: `app/database.py`.
