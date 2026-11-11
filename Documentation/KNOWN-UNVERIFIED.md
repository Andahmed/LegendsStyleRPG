# KNOWN-UNVERIFIED — حالة المشروع وما لم يثبت بعد

**تاريخ المراجعة:** 2026-11-11  
**المشروع المعتمد:** GameClient/  
**Unity:** 2022.3.62f3  
**الإصدار:** 1.2.0

هذا الملف يفصل بين ما هو موجود في المصدر، وما تم التحقق منه ساكنًا، وما يحتاج تشغيل Unity أو هاتفًا حقيقيًا. لا تعني مراجعة الملفات أو نجاح رفع GitHub أن اللعبة تعمل.

## 1. ما تم التحقق منه في شجرة GitHub

| العنصر | ما تم التحقق منه | ما لم يثبت |
|---|---|---|
| جذر مشروع Unity | GameClient يحتوي Assets وPackages وProjectSettings | فتحه في محرر Unity وتجميعه |
| إصدار Unity | ProjectVersion يثبت 2022.3.62f3 | سلامة كل APIs والتبعيات أثناء Compile |
| سكريبتات C# | 81 ملف C# تحت GameClient/Assets في شجرة المصدر وقت المراجعة | عدم وجود Compile Errors |
| موديلات 3D | 208 ملفات FBX: 150 Nature + 48 Modular Dungeon + 4 Animated Monsters + 6 RPG Characters | نجاح Unity Import والخامات والـ clips |
| صور وموارد | ملفات PNG/JPG/PSD موجودة في Assets | وضوحها النهائي داخل UI وعلى الهاتف |
| جسر الأصول | QuaterniusAssetLibrary يستدعي Resources للموديلات؛ وHeroCharacter وMonsterCharacter يستبدلان الجسم الإجرائي عند نجاح التحميل | ظهور النماذج وتحركها بصورة صحيحة |
| المدينة والديكور | DioramaHomeBuilder يستعمل أصول طبيعة وقطع ديكور، وDioramaGlbRuntimeLoader يحاول تحميل نماذج GLB المضمّنة | صحة كل نموذج ومكانه ومقاسه بعد التشغيل |
| إعداد البناء | BuildTools يحدد Android وIL2CPP وARMv7/ARM64 وLandscape وID موحدًا | APK ناجح |
| Workflow | GitHub Actions مهيأ لاستدعاء BuildTools.BuildAndroid | يعتمد على أسرار Unity، ولم يثبت بعد Artifact ناجح |

## 2. أصول 3D — تغطية وليست اكتمالًا فنيًا

- ستة موديلات RPG أساسية يعاد توزيعها على فئات الأبطال؛ ليس لكل واحد من 62 بطلًا موديل فريد.
- أربعة موديلات وحوش تستخدمها خريطة ربط لعدة أنواع وأعداء وزعماء؛ ليست كل الوحوش والزعماء موديلات مستقلة.
- الطبيعة وقطع Dungeon موجودة وتُحمّل من Resources في مواضع محددة.
- بعض أجزاء غرفة الديوراما والطاولة والتضاريس والعناصر الإجرائية ما زالت مبنية بأشكال Unity البدائية.
- وجود FBX في Git لا يثبت أن المواد أو الـ Animation Clips تم استيرادها أو عرضها بصورة صحيحة في Unity.

## 3. بناء Android وGitHub Actions

الإعدادات المرجعية في GameClient/Assets/Editor/BuildTools.cs:
- Unity 2022.3.62f3
- Application ID: com.ahmed.legendsstylerpg
- IL2CPP
- ARMv7 + ARM64
- Minimum API 23 / Target API 34
- Landscape
- APK: Builds/Android/LegendsRPG-v1.2.0.apk

المشغل السابق كان يتخطى البناء إذا لم يكن Repository variable ENABLE_GITHUB_UNITY_BUILD مساويًا لـ true. تم حذف هذا الشرط من التصميم المعتمد. Workflow الحالي يفحص أسرار UNITY_LICENSE وUNITY_EMAIL وUNITY_PASSWORD، ثم يحاول البناء.

لا يمكن قراءة قيم Secrets عبر فحص المستودع. يجب أن يضيفها مالك المستودع من Settings → Secrets and variables → Actions. لا توجد هنا شهادة أن APK أُنتج أو رُفع بعد.

## 4. اختبارات لم تُثبت بعد

- Unity Editor import وC# compile للمشروع كله.
- Play Mode: شاشة البداية والمدينة، ضغط المباني، فتح الشاشات، الدخول والخروج من المعركة.
- قتال كامل: حركة الفريق، استهداف الأعداء، الضرر، المهارات، Ultimate، الموت والنتيجة.
- كل Animation Clip على الشخصيات والوحوش بعد الاستيراد.
- سلامة الحفظ القديم بعد تغييرات المعرفات.
- Android APK build وinstall وlaunch على جهاز فعلي.
- اللمس، اتجاه Landscape، اختلافات نسب الشاشات، الأداء وFPS وRAM والحرارة.
- اتصال العميل بالـ FastAPI: طبقة الخادم موجودة، لكن العميل لا يعتمد عليه كمصدر حقيقة متصل في التشغيل الحالي.

## 5. مخاطر بيانات الحفظ القديمة

تغيرت معرفات بعض الأبطال المولّدين من نطاقات أوسع إلى gen_*_0..5. المعرفات القديمة gen_*_6 وgen_*_7 لا تقابل أبطالًا في القاعدة الحالية، وبعض الندرات تغيّرت. يجب اختبار حفظ v1.1 حقيقي قبل اعتماد ترقية البيانات؛ لا تحذف حفظ اللاعب بغرض الاختبار.

## 6. معيار إعلان النجاح

لا يعلن عن نجاح بناء Android إلا عند توفر:
1. GitHub Actions run بحالة Success.
2. خطوة Verify APK ناجحة.
3. Artifact LegendsRPG-Android-APK يحتوي ملف APK غير فارغ.
4. تثبيت وتشغيل APK على هاتف حقيقي مع تسجيل نتائج الاختبار.

نجاح APK لا يساوي اعتماد اللعبة كاملة؛ يلزم استكمال قائمة Documentation/TestingGuide.md.


## أحدث نتيجة GitHub Actions — 2026-11-11

- Commit المفحوص: 06249de63f11a5f53f929bbfde75ce6c42021815.
- Workflow run: 43083520873.
- النتيجة: Failed عند خطوة Validate Unity credentials.
- خطوة Build project تم تخطيها نتيجة فشل التحقق السابق؛ لم يبدأ Unity Compile أو Android packaging.
- GitHub أعاد عدد Artifacts = 0؛ لا يوجد APK في هذا التشغيل.
- النتيجة تثبت أن واحدًا أو أكثر من الأسرار المطلوبة لم يكن متاحًا للـWorkflow. لا يمكن قراءة قيم Secrets من تكامل GitHub، لذلك لا أجزم أيها تحديدًا ناقص.

الرابط: https://github.com/Andahmed/LegendsStyleRPG/actions/runs/43083520873
