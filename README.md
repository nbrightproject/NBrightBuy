# NBrightBuy
NBrightStore - E-Commerce for DNN (NBSv3)

Developer please read this to get started:

http://nbsdocs.nbrightproject.org/Documentation/Developerguide/DevSetup.aspx

v3.6.1
- Fix to category Product Select.
- Fix Tax Drop Down to display description, not value.
- Add apply tax to all products in a category.
- Redisplay missing add button for products.
- Activate Richtext for bespoke model fields.

v3.6.0
- Convert Product Admin to Razor.
- BREAKING CHANGE TO CUSTOM PRODUCT FIELD - The file "producfield.html" is used to create bespoke field inthe product addmin.  If you have used this file it will need to be converted to razor as  "productfield.cshtml"
- Allow custom field on the model, using the file called "modelfields.cshtml"
- Convert Client Admin to Razor.
- Add custom fields to Client Admin "clientfields.cshtml"
- REQUIRED min. v8.2.0.0 on NBrightTS  (Templating system)
