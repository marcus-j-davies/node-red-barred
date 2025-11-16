**v1.2.0**
 - **New Features**
   - You can now send an HTML body by settng a propety of `msg.isHTML:true`  
    in responses or sending to scanners - **note:** only the body content is expected,  
    not a whole HTML document (including the `body` tag).  
    This allows for much richer content.

   - if sending html, a vlaue of `{{BARRED.themeColor}}`  
    will get replaced with the configurations color setting.

   - `INFO` responses, can now support dropdowns.  
      Todo this simply send a property with an array.  
      ```js
      msg.status = 'INFO'
      msg.payload = {
        Item: 'string',
        Cost: 'number',
        Date: 'date',
        PlaceOfPurchase: ['Best Buy', 'Micro Center', 'Lowes', 'Amazon']
      }
      ```

  - **Changes**
    - The audio feedback swtich state is now restored, when relaunching the app

**v1.1.2**
 - **Fixes**
   - Typos

**v1.1.1**
 - **Fixes**
   - Correct Read Me Links
   - Add Warning

**v1.1.0**
 - **New Features**
   - Added a fully customisable Menu system


**v1.0.0**
 - Initial Release