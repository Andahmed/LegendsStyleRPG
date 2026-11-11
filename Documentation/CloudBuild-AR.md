# بناء Android — GitHub Actions وUnity Build Automation

## المسار المعتمد حاليًا: GitHub Actions

ملف Workflow:
.github/workflows/build-android.yml

يبدأ البناء تلقائيًا عند Push إلى main إذا تغيّر شيء داخل GameClient/** أو ملف Workflow، كما يمكن تشغيله يدويًا من:
GitHub → Actions → Build Unity Android APK → Run workflow.

### متطلبات الترخيص

أضف القيم التالية كـ Repository Actions secrets من:
Settings → Secrets and variables → Actions:

1. UNITY_LICENSE
2. UNITY_EMAIL
3. UNITY_PASSWORD

القيم لا تُطبع في السجل. يفحص Workflow وجودها أولًا، ويوقف التشغيل برسالة واضحة إذا كان أحدها غير موجود. لم يعد هناك شرط مخفي باسم ENABLE_GITHUB_UNITY_BUILD ولا حاجة إلى Repository variable بهذا الاسم.

### إعداد البناء الموحد

| الإعداد | القيمة |
|---|---|
| Unity project folder | GameClient |
| Unity version | 2022.3.62f3 |
| Target | Android APK |
| Build method | LegendsRPG.EditorTools.BuildTools.BuildAndroid |
| Application ID | com.ahmed.legendsstylerpg |
| Backend | IL2CPP |
| Architectures | ARMv7 + ARM64 |
| Minimum / Target API | 23 / 34 |
| Orientation | Landscape |
| Output | GameClient/Builds/Android/LegendsRPG-v1.2.0.apk |

تقوم BuildTools.BuildAndroid باستدعاء PreExport، وإنشاء مشهد Boot وتسجيله عند الحاجة، وضبط Player Settings، ثم تشغيل BuildPipeline. لا تعتبر الخطوة ناجحة إلا إذا كان تقرير Unity ناجحًا والـ APK موجودًا وغير فارغ. بعد ذلك يرفع Workflow Artifact باسم LegendsRPG-Android-APK.

## قراءة نتيجة GitHub Actions

- **Success مع Artifact:** Unity أنهى البناء والملف رُفع. ما زال يلزم تثبيته واختباره على هاتف.
- **فشل Validate Unity credentials:** أضف الـ Secret المذكور في رسالة الخطأ.
- **فشل Build project:** افتح سجل GameCI وابحث عن أول error CS أو أول Gradle/Android error؛ أصلح أول خطأ ثم أعد التشغيل.
- **فشل Verify APK:** لم ينتج ملف الخرج المتوقع رغم انتهاء خطوة Unity؛ راجع BuildTools ومسار Build.
- **Skipped:** راجع شروط Workflow القديمة أو سبب التخطي. النسخة الحالية لا تستخدم متغير تمكين اختياريًا.

## Unity Build Automation كبديل

إذا أردت تشغيل Unity Build Automation من لوحة Unity بدل GitHub Actions:

- Repository: Andahmed/LegendsStyleRPG
- Unity project folder: GameClient
- Unity version: 2022.3.62f3
- Platform: Android
- Build type: APK
- Pre-export method: LegendsRPG.EditorTools.BuildTools.PreExport

دع خدمة Build Automation تنفذ مرحلة البناء الافتراضية بعد Pre-export. لا تضف BuildTools.BuildAndroid كـ Build method في نفس الهدف إلا إذا كنت تقصد تشغيل BuildPipeline المخصص داخل الخدمة؛ لا تجمع بين مساري البناء بلا اختبار.

ملف Deployment/cloudbuild/cloudbuild-targets.yaml مرجع لإعدادات المشروع فقط، وليس إثباتًا أن API استوردته أو أن هدف Cloud Build تم إنشاؤه. يجب إدخال القيم من لوحة الخدمة والتحقق من أول تشغيل.

## الحالة المسجلة

لم يكن هناك APK ناجح مثبت في التسليم عند كتابة هذه الوثيقة. وكانت تشغيلات GitHub السابقة تتخطى المهمة بسبب شرط ENABLE_GITHUB_UNITY_BUILD. إزالة الشرط تفعّل محاولة البناء؛ لكنها لا توفر رخصة Unity ولا تضمن أن أول Unity Compile سيجتاز.
