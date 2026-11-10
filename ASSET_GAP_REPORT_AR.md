# تقرير مقارنة الأصول الناقصة — LegendsStyleRPG

**المستودع:** https://github.com/Andahmed/LegendsStyleRPG
**الفرع المفحوص:** `main`
**تاريخ الفحص:** 2026-11-09

## النتيجة الأساسية

الحزم الثلاث التي حددها صاحب المشروع مناسبة لتغطية جزء كبير من النقص البصري في اللعبة، لكنها لن تجعل اللعبة مطابقة 100% لـ **Legends Reborn**. يمكن استخدامها لصناعة تجربة Fantasy RPG ثلاثية الأبعاد قريبة في الحلقة العامة والجو البصري، مع الحفاظ على أسماء وشخصيات وواجهة وهوية أصلية.

لا توجد حاليًا ملفات `FBX` أو `OBJ` أو `GLB` أو `GLTF` أو `MAT` داخل `GameClient/Assets`. الموجود في الأصول هو أساسًا **710 PNG و4 JPG**. وبناءً على قراءة الكود، فإن المدينة والوحوش ما زالت تُنشأ بمكعبات وأشكال ومواد إجرائية:

- `CityBuilder.cs` ينشئ الأرض والمباني والأشجار/العناصر بصيغة إجرائية.
- `MonsterCharacter.cs` يضيف `CharacterAnimator` و`CharacterBodyFactory` بدل تحميل Prefab ثلاثي الأبعاد جاهز.
- `CharacterAnimator.cs` يصرّح أن الأنيميشن الإجرائي مؤقت إلى أن تصل Rig/Clips حقيقية.
- `MaterialLibrary.cs` ينشئ المواد برمجيًا.

## مقارنة الحزم المطلوبة

### 1. Quaternius LowPoly Animated Monsters

المصدر الرسمي:
https://quaternius.itch.io/lowpoly-animated-monsters

صفحة الحزمة تذكر أن فيها وحوشًا متحركة بصيغ `OBJ` و`FBX` و`Blend`، مع حركات مثل الضرب والهجوم والقفز والطيران والمشي، وأن الصفحة تعرض ترخيص CC0. صفحة Quaternius الرسمية البديلة تصفها بأنها حزمة من **4 وحوش متحركة**.

**المطلوب من القائمة:** Bat، Dragon، Skeleton، Slime، مع FBX والأنيميشن والخامات والتراخيص.

**المطابقة:**

- Bat: متوقع وجوده في الحزمة.
- Dragon: متوقع وجوده في الحزمة.
- Skeleton: متوقع وجوده في الحزمة.
- Slime: متوقع وجوده في الحزمة.
- FBX: متاح حسب صفحة المصدر.
- Animations: متاحة حسب صفحة المصدر، لكن يجب فحص أسماء الـ clips وHumanoid/Generic import داخل Unity.
- Textures/Materials: يجب فحص بنية ZIP بعد التنزيل؛ لا يجوز افتراض أن كل ملف Material منفصل أو أن الروابط تلقائية.

**الحكم:** هذه الحزمة هي الأعلى أولوية لأنها تستبدل مباشرة الأشكال البدائية للوحوش، لكنها لا تغطي كل وحوش اللعبة التسعة ولا زعماء البرج وحدها. يجب استخدام الأشكال الأربعة للأنواع الأساسية، ثم عمل Variants بالألوان والحجم والـ VFX، أو إضافة حزمة مجانية أخرى مرخّصة للزعماء.

### 2. Quaternius Ultimate Nature

المصدر الرسمي:
https://quaternius.itch.io/150-lowpoly-nature-models

صفحة المصدر تذكر **150 نموذج طبيعة** بصيغ `Blend` و`FBX` و`OBJ`، وتذكر CC0. صفحة Quaternius الرسمية البديلة تسميها Ultimate Nature Pack.

**المطلوب من القائمة:** أشجار، شجيرات، صخور، نباتات، أعشاب وعناصر بيئية.

**المطابقة:**

- الأشجار: نعم.
- الشجيرات والنباتات والأعشاب: نعم.
- الصخور: نعم.
- عناصر البيئة الطبيعية: نعم.
- FBX: متاح حسب صفحة المصدر.
- Textures/Materials: يجب فحصها داخل الحزمة وربطها بمواد Unity.

**الحكم:** مناسبة للمدينة والطرق والمناطق الخارجية، لكنها ستعطي مظهر Low-poly عام، وليس مظهر Legends Reborn الحرفي. تحتاج إلى توزيع مدروس، إضاءة، fog، ground materials وLOD حتى لا تبدو كأنها أصول موضوعة عشوائيًا.

### 3. Quaternius Modular Dungeon

المصدر الرسمي:
https://quaternius.itch.io/lowpoly-modular-dungeon-pack

صفحة المصدر تذكر أكثر من **45 قطعة Dungeon modular** بصيغ `FBX` و`OBJ` و`Blend`، مع CC0. صفحة Quaternius الرسمية البديلة تعرض الحزمة باسم Modular Dungeons Pack وتذكر أنها مجانية للاستخدام الشخصي والتجاري.

**المطلوب من القائمة:** أرضيات، جدران، أسقف، أبواب، أعمدة، صناديق وزخارف.

**المطابقة:**

- قطع البناء modular: نعم.
- الجدران والأرضيات والأسقف: نعم.
- الأبواب والأعمدة: نعم.
- الصناديق والزخارف: جزء منها متوقع، ويجب تأكيده من محتويات ZIP.
- FBX: متاح حسب صفحة المصدر.
- Materials/Textures: يجب فحصها واستيرادها، ثم إنشاء Prefabs قابلة لإعادة الاستخدام.

**الحكم:** مناسبة للزنازين والمراحل الداخلية، وليست بديلًا عن مدينة رئيسية كاملة أو ساحات PVP أو قاعة نقابة مفصلة.

## ما ينقص للوصول إلى تجربة قريبة جدًا

الأصول الثلاثة تعالج البيئة والوحوش فقط. للوصول إلى تجربة قوية قريبة من أسلوب Legends Reborn ما زلنا نحتاج إلى:

1. **شخصيات أبطال Fantasy متحركة:** الحزم الثلاث لا توفر 62 بطلًا بأنيميشن وملابس ومهارات Ultimate.
2. **أنيميشن موحّد للأبطال:** Idle، walk، attack، hit، death، skill cast وUltimate.
3. **زعماء مميزون:** Dragon واحد أو Variants لونية لا يكفي لصنع Bosses مختلفة بصريًا.
4. **VFX للمهارات:** انفجارات، نار، جليد، برق، نور، ظلام، دوائر استدعاء وضربات Ultimate.
5. **واجهة RPG أصلية:** portraits، rarity frames، stars، health/rage bars، summon reveal، equipment screen وbattle result.
6. **مباني المدينة:** الحزم الطبيعية لا توفر Fountain، Summon building، Guild Hall، Forge، Shop، Arena أو بوابة الحملة كاملة.
7. **صوت وموسيقى:** الحزم لا تحتوي على تجربة صوتية كاملة للقتال أو المدينة.
8. **تحسين Android:** LOD، batching، atlas، texture compression، pooling وقياس FPS/الذاكرة على جهاز حقيقي.

لذلك، الهدف الواقعي هو: **نفس نوع الحلقة والوضوح البصري، وليس نسخة مطابقة أو إعادة إنتاج للعبة التجارية.**

## وضع المستودع الحالي

- لا يوجد في الفرع `main` ملفات 3D جاهزة من الحزم الثلاث.
- مجلد `GameClient/Assets/GameArt/` يحتوي README وتعليمات عامة، وليس مكتبة FBX مكتملة.
- README الحالي يحذر من رفع أصول Asset Store الخام إلى مستودع عام.
- الحزم المطلوبة من Quaternius ليست بالضرورة مشكلة واحدة من ناحية الترخيص، لأن صفحاتها تعرض CC0، لكن صفحة الترخيص الحالية لـ Quaternius تمنع إعادة توزيع الأصول نفسها كحزمة مستقلة. لذلك لا ينبغي رفع ZIPs أو مكتبة خام كاملة إلى مستودع GitHub عام بدون التحقق من الترخيص الذي كان ساريًا عند الحصول على كل حزمة.
- الآمن عمليًا هو وضع الأصول في مستودع خاص أو استخدامها داخل مشروع/Build نهائي، مع حفظ ملف الترخيص والإسناد، وعدم نشر الحزمة الخام كمكتبة منفصلة.

## قرار الدمج المقترح

### المرحلة A — الوحوش

- تنزيل الحزمة من الصفحة الرسمية فقط.
- فحص ZIP وتسجيل الملفات الموجودة فعليًا.
- استخراج FBX والخامات وملفات الترخيص إلى مجلد عمل غير منشور.
- إنشاء Prefabs داخل Unity باسم واضح لكل وحش.
- ربط `MonsterCharacter` بطريقة تحميل Prefab مع fallback إلى المجسم الإجرائي عند غياب الأصل.
- إنشاء mapping للأنيميشن بدل تغيير نظام القتال.
- اختبار spawn، chase، attack، hit، death وboss scale.

### المرحلة B — الطبيعة

- استيراد مجموعة صغيرة فقط من الأشجار والصخور والأعشاب المستخدمة فعلًا.
- وضعها في `CityBuilder` عبر Prefab slots أو Asset mapping.
- إضافة LOD وcollider بسيط للأجسام القابلة للتفاعل.
- عدم وضع 150 نموذجًا في Resources بلا استخدام، لأن ذلك يزيد حجم المشروع ووقت الاستيراد.

### المرحلة C — الـ Dungeon

- بناء مجموعة Prefabs modular.
- تجهيز Dungeon test scene داخل Unity.
- استخدام نفس `BattleSession` ونظام القتال الحالي.
- عدم تغيير أسماء الأنظمة أو APIs الحالية.

### المرحلة D — التحقق

- فتح المشروع في Unity 2022.3.55f1.
- التأكد من عدم وجود Compile Errors.
- فحص import settings للـ FBX والـ Animation Clips.
- تشغيل معركة كاملة.
- اختبار المدينة والدنجن والوحش والـ Ultimate.
- بناء APK Android.
- فحص FPS والذاكرة ودرجة الحرارة على جهاز حقيقي.

## لا يمكن تأكيده الآن

لا يمكن تأكيد عدد الملفات النهائي أو عدد الدفعات أو روابط Commits قبل توفر ملفات ZIP نفسها في بيئة العمل ثم استخراجها وفحصها. كذلك لا يمكن الادعاء أن الدمج اكتمل بدون Unity import/build فعلي.

## المصادر

[1]: https://quaternius.itch.io/lowpoly-animated-monsters "LowPoly Animated Monsters — Quaternius"
[2]: https://quaternius.itch.io/150-lowpoly-nature-models "150+ LowPoly Nature Models — Quaternius"
[3]: https://quaternius.itch.io/lowpoly-modular-dungeon-pack "LowPoly Modular Dungeon Pack — Quaternius"
[4]: https://quaternius.com/license.html "Quaternius Asset License"
[5]: https://creativecommons.org/publicdomain/zero/1.0/ "Creative Commons CC0 1.0"
[6]: https://github.com/Andahmed/LegendsStyleRPG/blob/main/GameClient/Assets/GameArt/README.txt "LegendsStyleRPG GameArt README"
