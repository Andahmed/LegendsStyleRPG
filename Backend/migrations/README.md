# migrations/ — ترحيلات قاعدة البيانات

للتطوير تُنشأ الجداول تلقائيًا (`init_db`). للإنتاج يوصى بـ Alembic:

```bash
pip install alembic
alembic init migrations
# اربط app.models.Base.metadata في env.py ثم:
alembic revision --autogenerate -m "init"
alembic upgrade head
```

لا تُطبَّق ترحيلات على الإنتاج قبل نسخة احتياطية موثقة.
