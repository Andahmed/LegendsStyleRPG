# LegendsStyleRPG — Unity Android

هذا هو جذر مشروع Unity المعتمد. افتح هذا المجلد من Unity Hub، وليس جذر مستودع GitHub.

- Unity Editor: **2022.3.62f3**
- Build target: Android APK
- Application ID: com.ahmed.legendsstylerpg
- Backend: IL2CPP
- Architectures: ARMv7 + ARM64
- Minimum / Target API: 23 / 34
- Orientation: Landscape (left/right)

## فتح وتشغيل

1. ثبّت Unity 2022.3.62f3 مع Android Build Support وSDK/NDK وOpenJDK.
2. Unity Hub → Add project from disk → اختر مجلد GameClient.
3. انتظر استيراد الأصول وتجميع C#.
4. افتح Assets/Scenes/Boot.unity واضغط Play. إذا لم يوجد المشهد، ينشئه ProjectBootstrapEditor تلقائيًا عند تحميل المشروع.

## البناء

من قائمة Unity: **Legends RPG → Build → Build Android APK**.

الخرج:
Builds/Android/LegendsRPG-v1.2.0.apk

GitHub Actions يستخدم نفس نقطة الدخول LegendsRPG.EditorTools.BuildTools.BuildAndroid. راجع ../Documentation/CloudBuild-AR.md لإعداد Secrets وسجل البناء.

## محتوى المصدر

يوجد 62 بطلًا وفريق من 6 أبطال، قتال تلقائي، حملة وبرج وأنظمة تقدم ومعدات، إضافة إلى 208 ملفات FBX ضمن Assets/Resources/GameArt/Free3D. كود التشغيل يستخدم هذه الأصول عند توفرها ويحتفظ بأجسام إجرائية احتياطية.

**تنبيه:** لا يوجد إثبات تشغيل فعلي لمجرد نجاح رفع الملفات إلى GitHub. راجع ../Documentation/KNOWN-UNVERIFIED.md قبل اعتبار أي وظيفة مختبرة.
