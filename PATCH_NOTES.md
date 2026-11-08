# حزمة الإصلاحات

انسخ المجلدات فوق جذر المشروع (LegendsStyleRPG/) واستبدل الملفات.

## v0.5 — دمج الفرعين (v0.4 الأرت كيت/الهَب/السحب + شاشة البطل والمعدات)

| الملف | التغيير |
|---|---|
| Core/Economy.cs | نظام المعدات (4 خانات لكل بطل: سلاح/خوذة/درع/حذاء) بمكافآت حقيقية للإحصائيات + 7 قطع معدات + Inventory.SpendGold/HeroLevel/SetHeroLevel |
| Core/SaveSystem.cs | حقول جديدة: مستوى كل بطل + المعدات المركّبة (تتحفظ، وتتحمّل RebuildTeam) |
| Team/TeamManager.cs | كل بطل يتولّد بمستواه المحفوظ + تتطبّق عليه معداته + حفظ المستوى عند الترقية |
| UI/Menu/MenuController.cs | شاشة البطل الكاملة: خانات معدات + اختيار من الحقيبة + إحصائيات + مهارات + LEVEL UP بالذهب، وزر EQUIP / LEVEL UP في لوحة تفاصيل البطل |
| Editor/ProjectBootstrapEditor.cs | version 0.5.0 + versionCode 11 |

ملاحظات الدمج: السحب (GachaSystem/SummonScreen) والهب والأرت كيت والـ 20 بطل اتحافظ عليهم كما هم من v0.4، واتشالت نسختي البسيطة من السحب والشاشة الرئيسية لأن نسخة v0.4 أكمل.

## v0.4 — سحب الأبطال (Gacha) + الـ Art Kit

| الملف | التغيير |
|---|---|
| GameClient/Assets/Scripts/Art/ArtKit.cs | جديد: نظام سلوتات الأرت (ArtSlots + ArtSlotMap + ArtLibrary) — الموديلات الحقيقية بدل الأشكال البدائية مع fallback آمن |
| GameClient/Assets/Editor/ArtSlotsWindow.cs | جديد: نافذة Legends RPG > Art > Art Slots (Auto-match + سحب وإفلات + حفظ في Resources) |
| GameClient/Assets/Scripts/Core/GachaSystem.cs | جديد: 3 بانرات سحب (Basic/Advanced/Supreme) بالنِسَب والـ pity والضمانة على x10 والويش بعد 60 سحبة، والمكرر بيتحول لـ Hero Essence + ذهب |
| GameClient/Assets/Scripts/UI/Menu/SummonScreen.cs | جديد: شاشة السحب (تبويبات البانرات، عداد الـ pity، أزرار x1/x10، عرض النتائج ببطاقات تتكشف واحدة واحدة) |
| GameClient/Assets/Scripts/Characters/HeroDatabase.cs | الترخيزة كبرت من 6 لـ **20 بطل** (3 Common، 5 Rare، 7 Epic، 4 Legendary، 1 Mythic) + كاش AllHeroes |
| GameClient/Assets/Scripts/Team/TeamManager.cs | Roster + تشكيلة الفريق المحفوظة (teamLoadout) + RebuildTeam() بعد السحب أو التعديل |
| GameClient/Assets/Scripts/UI/Menu/MenuController.cs | زر SUMMON في القائمة + شاشة الأبطال بقت تعرض الترخيزة كاملة مع كروت مقفولة وزر إضافة/إزالة من الفريق |
| GameClient/Assets/Scripts/UI/Menu/HubScreen.cs | مذبح الاستدعاء في الهب فاتح شاشة السحب فعلاً |
| GameClient/Assets/Scripts/Core/SaveSystem.cs | schema 3: pity + wish + teamLoadout + هدية إطلاق 3200 جوهرة للحسابات القديمة (مرة واحدة) |
| GameClient/Assets/Scripts/Core/Economy.cs | عنصر Hero Essence (تبويب Shards) |
| GameClient/Assets/Scripts/Characters/CharacterBodyFactory.cs | بيدور على موديل الـ Art Kit الأول ويرجع للأشكال البدائية لو مفيش |
| GameClient/Assets/Editor/ProjectBootstrapEditor.cs | version 0.4.0 + versionCode 10 + اسم ملف اللعبة LegendsRPG-v0.4.0.apk |
| .github/workflows/build-android.yml | مطابقة نسخة يونيتي للـ CI مع نسختك المحلية 2022.3.62f3 |
| Documentation/ArtKit-AR.md | جديد: دليل عربي كامل لاستيراد مكتباتك + خطوات البناء + شرح حدود الحجم على Play |

## v0.2-hub — الشاشة الرئيسية (الهب) + جودة الرسومات

| الملف | التغيير |
|---|---|
| GameClient/Assets/Scripts/World/HubScene.cs | جديد: القارة ثلاثية الأبعاد القابلة للضغط (10 مبانٍ + بوابة المعركة) |
| GameClient/Assets/Scripts/UI/Menu/HubScreen.cs | جديد: الشاشة الرئيسية (كارت اللاعب، عملات، أحداث، شريط سفلي، صندوق AFK) |
| GameClient/Assets/Scripts/Core/GameRoot.cs | الخطوة 11: فتح الهب كشاشة رئيسية مع ربط أزرار الإدارة بـ MenuController |
| GameClient/Assets/Scripts/UI/Menu/MenuController.cs | إضافة ShowScreen(id) للانتقال المباشر من الهب إلى Missions/Heroes/Bag/Shop |
| GameClient/Assets/Scripts/Core/GraphicsQualityBootstrap.cs | جديد: 60 FPS، منع نوم الشاشة، إطفاء الظلال الثقيلة، كاميرا أمان |
| GameClient/Assets/Editor/ProjectBootstrapEditor.cs | bundleVersion=0.2.0 و versionCode=8 (الترقية فوق الـ APK المنصّب 7) |

| الملف | الإصلاح |
|---|---|
| GameClient/Packages/manifest.json | يضيف unitywebrequest (يحل أخطاء ApiClient.cs) |
| Backend/app/models.py | DateTime بدون timezone: على PostgreSQL المقارنة كانت تفشل (naive vs aware) وتكسر كل الـ tokens |
| Backend/requirements.txt | إضافة psycopg (بدونه prod.env لا يعمل) |
| Backend/app/config.py | تبسيط منطق DATABASE_URL |
| Backend/app/main.py | lifespan بدل on_event + منع credentials مع CORS "*" |
| Backend/app/api/routes_auth.py | تنظيف التوكنات بـ DELETE واحد |
| Deployment/docker/Dockerfile | مجلد data قابل للكتابة لمستخدم appuser |
| Deployment/docker/docker-compose.yml | يقرأ dev.env (مطابق للتوثيق) |
| Deployment/docker/docker-compose.prod.yml | جديد: PostgreSQL + prod.env |
| Deployment/config/dev.env.example | SQLite داخل الـ volume |
| .gitignore | إصلاح Builds/ و منع رفع *.env الحقيقية |

ملاحظة: ربط Unity Cloud على 2022.3.55f1 يتم من لوحة Unity Cloud وليس من الملفات.

## v0.5.1 — UI Skin (رسومات الواجهة بدون Unity)

| الملف | التغيير |
|---|---|
| Scripts/UI/Menu/UISkin.cs | جديد: بيدوّر على PNG في Resources/UISkin بالاسم (أسماء Kenney شغالة) ويرجع للشكل المرسوم لو مفيش |
| Editor/UISkinImporter.cs | جديد: أي صورة في Resources/UISkin بتتحوّل Sprite أوتوماتيك + حواف 9-slice (يشتغل في Cloud Build حتى من غير .meta) |
| Scripts/UI/Menu/MenuTheme.cs | الأزرار والبانلز بتستخدم الـ Skin لو موجود |
| Documentation/UISkin-AR.md | خطوات الرفع من الموبايل عبر GitHub |
