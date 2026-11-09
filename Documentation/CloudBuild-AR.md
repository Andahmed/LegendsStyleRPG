# الإعداد على Unity Cloud Build (Build Automation) — خطوة بخطوة

الهدف: كل ضغطة على GitHub تنتج APK بدون فتح Unity على جهازك.

> صراحة: هذه الخطوات كُتبت لتطابق إعدادات المشروع الحالية (ProjectVersion.txt =
> 2022.3.55f1) لكن **لم تُجرَّب فعليًا على Cloud Build** — أول تشغيل قد يحتاج
> تعديلًا بسيطًا، والخطأ الأول المتوقع دائمًا هو خطأ كومبايل يظهر في أول 30 سطرًا من اللوج.

## 1) إنشاء الهدف (Build Target)

1. ادخل [Unity Cloud](https://cloud.unity.com) → **Build Automation** → **Add project**
   واربط مستودع GitHub الذي فيه هذا المشروع.
2. **Project folder**: `GameClient` (المشروع داخل هذا المجلد — ليس جذر المستودع).
3. **Unity version**: اختر **2022.3.55f1** (نفس `GameClient/ProjectSettings/ProjectVersion.txt`).
   لو اختلفت النسخة عن ProjectVersion.txt سيحاول Cloud Build التحويل ويفشل غالبًا.
4. **Platform**: Android.
5. **Build target type**: Android (APK).

## 2) الإعدادات الحاسمة في الهدف

| الإعداد | القيمة | السبب |
|---|---|---|
| Pre-export method | `LegendsRPG.EditorTools.BuildTools.PreExport` | يولّد `Assets/Scenes/Boot.unity` في batchmode ويضبط إعدادات اللاعب (IL2CPP/ARM64/API 23-34/com.legends.stylerpg) |
| Build method | `LegendsRPG.EditorTools.BuildTools.BuildAndroid` *(اختياري)* | يستخدم نفس المسار فينتج `Builds/Android/LegendsRPG-v1.1.0.apk` ويطبع سبب الفشل في اللوج |
| Auto-build | فعّله لكل push على الفرع الرئيسي | بناء مستمر |

ملاحظة: حتى لو كتبت **Build method** فاضية، Cloud Build يستخدم `EditorBuildSettings`
التي يظبطها `PreExport` — الطريقتان تعملان.

## 3) ماذا يفعل PreExport بالضبط؟

1. لو `Assets/Scenes/Boot.unity` غير موجود: ينشئ مشهدًا فيه `GameRoot` + ضوء
   اتجاهي ويحفظه (يعمل في batchmode بدون نافذة).
2. يسجّل المشهد في EditorBuildSettings.
3. يضبط: `com.legends.stylerpg`، Scripting Backend = **IL2CPP**،
   **ARM64 فقط**، minSdk = **23**، targetSdk = **34**، الاتجاه = **Portrait**
   (مطابق لكود `GameRoot` نفسه — لو أردت Landscape يجب تغيير الاثنين معًا).

## 4) لو فشل البناء

1. افتح لوج البناء في Cloud Build وابحث عن أول:
   `error CS...` (خطأ كومبايل) أو `BuildFailedException`.
2. أكثر الأسباب شيوعًا:
   - خطأ كومبايل في سكربت — انسخ أول رسالة وأصلحها ثم أعد الـ push.
   - نسخة Unity المختارة لا تملك وحدة Android Build Support — غيّر النسخة إلى 2022.3.55f1.
   - `Pre-export method` مكتوبة باسم خاطئ — يجب أن تكون كاملة بالـ namespace كما في الجدول أعلاه.
3. البناء يخرج برمز خروج 1 عند الفشل (`EditorApplication.Exit(1)`) فيظهر أحمر تلقائيًا.

## 5) رفع رقم الإصدار

- `bundleVersion` و`versionCode` مضبوطة داخل
  `GameClient/Assets/Editor/BuildTools.cs` (`ApplyAndroidPlayerSettings`).
- قبل كل نشر جديد: ارفع `bundleVersionCode` (+1) في نفس الدالة، وعدّل اسم الـ APK
  في `ApkName` لو أردت.
