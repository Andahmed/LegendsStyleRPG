# LegendsStyleRPG — لعبة RPG Fantasy ثلاثية الأبعاد للموبايل

مشروع لعبة RPG ثلاثية الأبعاد لنظام Android مستوحى من أسلوب Legends Reborn، بعالم وشخصيات وأنظمة أصلية. هذا المستودع يحتوي التسليم الكامل: عميل Unity + Backend Python + التوثيق + اختبارات.

## هيكل التسليم

```
LegendsStyleRPG/
├── GameClient/        ← مشروع Unity Android (افتحه بـ Unity Hub)
├── Backend/           ← خدمة Python/FastAPI (حسابات، حفظ سحابي، اقتصاد لاحقًا)
├── Deployment/        ← Docker + إعدادات التطوير/الإنتاج
├── Tests/             ← تقارير الاختبار ولقطات الشاشة
├── Builds/Android/    ← تُكتب هنا مخرجات APK
└── Documentation/     ← Architecture / Installation / ServerSetup / TestingGuide
```

## البداية السريعة

| أريد أن... | اذهب إلى |
|-----------|----------|
| أبني APK على هاتفي | `Documentation/Installation.md` (دليل من الصفر) |
| أفهم بنية الكود | `Documentation/Architecture.md` |
| أشغّل السيرفر | `Documentation/ServerSetup.md` + `Backend/README.md` |
| أعتمد تسليم مرحلة | `Documentation/TestingGuide.md` + ملف Excel المرفق بالتسليم |

## الإصدار الحالي

- **v0.1.0-prototype** — Prototype 0.1 كامل + أساسات 0.3 (فريق/تبديل/AI/Auto Battle) و0.4 (إحصائيات ومعدات قابلة للتطبيق) و0.5 (برج 4×200 طابق).
- إصدار Unity الموثق: **2022.3.55f1** — Minimum API 23، Target API 34.
- الحسابات والاقتصاد والـ Online: مبنيان كخدمة Backend مختبرة؛ الربط من داخل اللعبة في مراحل لاحقة كما في خطة التكليف.
