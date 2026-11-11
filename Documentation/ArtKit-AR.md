# دليل الأصول الفنية — LegendsStyleRPG

## 1. الأصول المرفقة في النسخة الحالية

الأصول الأساسية من Quaternius موجودة بالفعل داخل مشروع Unity:
GameClient/Assets/Resources/GameArt/Free3D/

| المجموعة | FBX |
|---|---:|
| Nature | 150 |
| Modular Dungeon | 48 |
| Animated Monsters | 4 |
| RPG Characters | 6 |
| **الإجمالي** | **208** |

لا تحتاج نسخة المستودع الحالية إلى تنزيل الحزم الأربع مرة أخرى. لا تنسخ ZIP الخام فوق المجلد الموجود.

## 2. كيف يستعملها الكود

- QuaterniusAssetLibrary يحمل الموديلات من Resources.
- HeroCharacter يحاول تحميل موديل الفئة؛ إذا نجح يخفي الجسم الإجرائي، وإذا فشل يحتفظ بالـ fallback.
- MonsterCharacter يحمل أحد موديلات الوحوش الأربعة ويطبق لون العنصر/الزعيم.
- DioramaHomeBuilder يستخدم موديلات طبيعة وبعض قطع Dungeon في المعالم.
- ArenaBuilder يضيف قطع Dungeon إلى ساحة القتال.
- QuaterniusFbxImporter يضبط استيراد FBX والأنيميشن إلى Generic.

الموديلات الستة للشخصيات تمثل فئات، وليست موديلًا فريدًا لكل بطل. والأشكال الأربعة للوحوش يعاد استخدامها بين معرفات مختلفة. هذا يعني أن التخصيص البصري ما زال مطلوبًا.

## 3. التحقق داخل Unity

بعد فتح GameClient في Unity 2022.3.62f3:

1. انتظر اكتمال Import لجميع ملفات FBX.
2. راجع Console بحثًا عن Import أو Material أو Animation errors.
3. افتح FBX وتحقق من Rig/Generic Animation Clips والخامات.
4. شغّل Play وافحص الأبطال والوحوش والطبيعة وقطع Dungeon.
5. تأكد أن الموديل الحقيقي يظهر بدل الجسم الإجرائي، وأن الجسم الإجرائي لا يتداخل معه.
6. اختبر Idle/Walk/Attack/Death وأي Clips متاحة. أسماء المقاطع التي يبحث عنها QuaterniusClipDriver هي Idle وWalk/Flying وAttack/Punch/Hit وCast/Skill وDeath.
7. سجّل أي نموذج لا يحمل أو لا يملك المقاطع المتوقعة؛ لا تعتبر fallback دليلًا على نجاح الأصل.

## 4. Art Slots للأصول الإضافية

نافذة Legends RPG → Art → Art Slots مخصصة لموديلات إضافية/مستوردة يدويًا. تبحث عن Prefabs تحت Assets/GameArt وتكتب الربط في Assets/Resources/ArtSlotMap.asset.

هذه الآلية منفصلة عن Quaternius Resources bridge. لا تفترض أن الضغط على AUTO-MATCH يستورد ملفات FBX أو ينشئ Prefabs تلقائيًا.

## 5. الترخيص

راجع Art/CREDITS.md وشروط كل أصل قبل التوزيع. لا ترفع حزم Asset Store المدفوعة أو ملفاتها الخام إلى مستودع عام دون إذن صريح. وجود سجل Credits لا يغني عن التحقق من حق إعادة التوزيع.

## 6. الحالة الصادقة

ملفات FBX موجودة في شجرة Git، وكود الربط موجود. لكن لم يثبت ذلك وحده أن Unity استوردها بنجاح أو أن شكلها النهائي والأنيميشن والأداء اجتازوا الاختبار. انظر KNOWN-UNVERIFIED.md.
