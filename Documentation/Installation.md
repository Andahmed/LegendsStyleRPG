# تثبيت LegendsStyleRPG وبناء Android APK

هذا الدليل يطابق إعدادات المصدر الحالية، وليس تعليمات الإصدارات القديمة.

## 1. المتطلبات

- Unity Hub.
- Unity Editor **2022.3.62f3** بالضبط، كما هو مثبت في GameClient/ProjectSettings/ProjectVersion.txt.
- من وحدات Unity: Android Build Support وAndroid SDK & NDK Tools وOpenJDK.
- مساحة فارغة كافية لاستيراد ملفات FBX وبناء Android.

## 2. فتح المشروع الصحيح

1. نزّل المستودع Andahmed/LegendsStyleRPG أو استخدم Clone.
2. من Unity Hub اختر **Add project from disk**.
3. اختر مجلد GameClient تحديدًا؛ لا تختَر جذر GitHub.
4. افتح المشروع بإصدار Unity 2022.3.62f3.
5. انتظر حتى ينتهي Package Manager من تنزيل الحزم وحتى يكتمل استيراد Assets.
6. افتح Console. لا تكمل الاختبار إذا توجد أخطاء حمراء (Compile Errors).
7. ينشئ ProjectBootstrapEditor مشهد Assets/Scenes/Boot.unity تلقائيًا عند تحميل المشروع إذا كان غير موجود، ويضيفه إلى Build Settings.

## 3. إعدادات Android المرجعية

تُطبّق تلقائيًا من Assets/Editor/BuildTools.cs عند البناء:

| الإعداد | القيمة |
|---|---|
| Application ID | com.ahmed.legendsstylerpg |
| Scripting Backend | IL2CPP |
| Architectures | ARMv7 + ARM64 |
| Minimum API | 23 |
| Target API | 34 |
| Orientation | Landscape فقط، يسار أو يمين |
| Bundle version | 1.2.0 |
| Version code | 14 |

أي تعديل على هذه القيم يجب أن يتم في BuildTools.ApplyAndroidPlayerSettings ثم تُراجع الوثائق معه.

## 4. تجربة المشروع داخل Unity

1. افتح Assets/Scenes/Boot.unity.
2. اضغط Play.
3. افحص ظهور شاشة/مدينة البداية ثم التنقل إلى الأنظمة.
4. ابدأ معركة وتحقق من حركة الأبطال واستهدافهم وهجماتهم والمهارات والـ Ultimate.
5. اختبر الإيقاف واستئناف السرعة x1/x2/x3، والعودة من المعركة.
6. افتح Console وراجع أي Missing Resource أو Animation أو NullReference.

لا تعتبر أي بند ناجحًا لمجرد ظهور المشهد؛ سجّل النتيجة وفق Documentation/TestingGuide.md.

## 5. أصول 3D

ملفات Quaternius الحالية موجودة تحت:
Assets/Resources/GameArt/Free3D/

وتتكون من 208 ملفات FBX: 150 Nature و48 Modular Dungeon و4 Animated Monsters و6 RPG Characters. يستدعيها QuaterniusAssetLibrary من Resources، وتضبط QuaterniusFbxImporter نوع الاستيراد والـ Animation.

لا تحتاج إلى إعادة تنزيل هذه الملفات من Asset Store عند استخدام نسخة المستودع الحالية. لكن يجب التحقق داخل Unity من نجاح الاستيراد والخامات والـ Animation Clips قبل اعتمادها بصريًا.

## 6. بناء APK محليًا

1. افتح المشروع في Unity 2022.3.62f3.
2. انتظر انتهاء الاستيراد والتجميع دون Compile Errors.
3. من القائمة اختر **Legends RPG → Build → Build Android APK**.
4. يقوم BuildTools بإنشاء Boot scene عند الحاجة وضبط إعدادات Android وبناء الملف.
5. مسار الخرج: Builds/Android/LegendsRPG-v1.2.0.apk
6. لا يعتبر البناء ناجحًا إلا إذا أظهر Console رسالة APK built successfully وكان الملف موجودًا وغير فارغ.

## 7. البناء عبر GitHub Actions

Workflow: .github/workflows/build-android.yml

أضف هذه القيم كـ Repository secrets من:
**Settings → Secrets and variables → Actions → New repository secret**

- UNITY_LICENSE
- UNITY_EMAIL
- UNITY_PASSWORD

بعدها ادفع تغييرًا إلى main داخل GameClient/ أو شغّل Workflow يدويًا من Actions. لا يلزم إنشاء Repository variable باسم ENABLE_GITHUB_UNITY_BUILD.

بعد النجاح، افتح تشغيل Workflow ونزّل Artifact باسم LegendsRPG-Android-APK. إذا فشل فاقرأ أول خطأ في Unity log؛ لا تُعامل فشل البناء كاختبار ناجح.

## 8. التثبيت والاختبار على الهاتف

يمكن التثبيت باستخدام Android Debug Bridge:

- adb install -r Builds/Android/LegendsRPG-v1.2.0.apk
- adb logcat -s Unity

اختبر على الأقل:
- بدء التشغيل دون انهيار.
- اتجاه Landscape على الهاتف.
- تحميل الموديلات والـ Animation Clips والخامات.
- انتقالات الشاشات والضغط على معالم المدينة.
- معركة كاملة من البداية للنهاية.
- عدم تداخل الواجهة مع أبعاد الشاشة.
- الأداء والذاكرة والحرارة على جهاز حقيقي.

## 9. حدود التسليم الحالية

راجع KNOWN-UNVERIFIED.md: وجود الكود والأصول وإعدادات CI لا يثبت أن Unity Compile أو Play Mode أو جهاز Android قد اجتاز الاختبار. لا تعتمد الإصدار قبل وجود APK ناجح ونتيجة اختبار هاتف موثقة.
