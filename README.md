# LegendsStyleRPG — Unity Android

المجلد الوحيد المعتمد كمشروع Unity هو **GameClient/** داخل المستودع. لا تفتح جذر المستودع كمشروع Unity.

## Baseline المعتمد

| الإعداد | القيمة |
|---|---|
| الإصدار | 1.2.0 |
| Unity Editor | 2022.3.62f3 |
| مشروع Unity | GameClient |
| المنصة | Android APK |
| Application ID | com.ahmed.legendsstylerpg |
| Scripting Backend | IL2CPP |
| Architectures | ARMv7 + ARM64 |
| Minimum / Target API | 23 / 34 |
| اتجاه العرض | Landscape فقط (يسمح باليمين واليسار) |

هذه القيم مأخوذة من GameClient/ProjectSettings/ProjectVersion.txt وGameClient/Assets/Editor/BuildTools.cs.

## محتوى اللعبة الموجود في المصدر

- 62 بطلًا في HeroDatabase، وفريق من 6 أبطال.
- قتال تلقائي قائم على TeamAIBrain وAutoSkills، مع Ultimate من البورتريه، وسرعات x1/x2/x3 وإيقاف مؤقت.
- حملة من 12 فصلًا × 15 مرحلة، وبرج من 4 درجات صعوبة × 200 طابق.
- اقتصاد وتقدم ومهام ومعدات ونظام نقابة وأنظمة Meta متعددة.
- 235 عنصر معدات محسوبة من قوالب وندرات.
- أصول 3D مرفقة داخل GameClient/Assets/Resources/GameArt/Free3D: 208 ملفات FBX (150 طبيعة، 48 Dungeon، 4 وحوش متحركة، 6 شخصيات).
- كود QuaterniusAssetLibrary يحمّل موديلات الأبطال والوحوش والطبيعة وقطع Dungeon، مع أجسام إجرائية احتياطية عند فشل التحميل. لا يعني وجود الملفات أن استيراد Unity أو العرض النهائي قد تم اختباره.
- الواجهة والعالم وبعض عناصر الديوراما لا تزال تُنشأ برمجيًا؛ ليست كل عناصر البيئة موديلات منفصلة.

## فتح المشروع

1. ثبّت Unity 2022.3.62f3 مع Android Build Support وAndroid SDK & NDK Tools وOpenJDK.
2. Unity Hub → Add project from disk → اختر مجلد GameClient.
3. انتظر اكتمال استيراد الأصول وتجميع C#.
4. ينشئ ProjectBootstrapEditor مشهد Assets/Scenes/Boot.unity تلقائيًا ويضيفه إلى Build Settings.
5. افتح Boot.unity واضغط Play لاختبار المحرر.

## بناء APK محليًا

من قائمة Unity: **Legends RPG → Build → Build Android APK**.

ملف الخرج المتوقع:
GameClient/Builds/Android/LegendsRPG-v1.2.0.apk

## البناء عبر GitHub Actions

Workflow: .github/workflows/build-android.yml

- يعمل عند Push إلى main إذا تغيّر مشروع GameClient أو ملف Workflow، ويمكن تشغيله يدويًا من Actions.
- يحتاج Repository Actions secrets بالأسماء: UNITY_LICENSE وUNITY_EMAIL وUNITY_PASSWORD.
- لا يحتاج Repository variable باسم ENABLE_GITHUB_UNITY_BUILD.
- لا تعتبر العملية ناجحة إلا بعد اكتمال خطوة Unity Build وظهور Artifact باسم LegendsRPG-Android-APK.

## حالة الاختبار الصادقة

المصدر والأصول وإعدادات البناء موجودة في GitHub. لكن لا يوجد ادعاء بأن C# Compile أو Play Mode أو اختبار الهاتف أو APK ناجح قد تم حتى يظهر سجل تشغيل ناجح وArtifact قابل للتنزيل. راجع Documentation/KNOWN-UNVERIFIED.md وDocumentation/TestingGuide.md.

## Backend

خادم FastAPI موجود كجزء منفصل من المشروع. العميل يعمل محليًا/Offline في الوضع الحالي؛ وجود الخادم لا يعني أن تسجيل الدخول أو الحفظ السحابي أو الاقتصاد أصبح متصلًا به.
