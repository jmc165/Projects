using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NewsPrint80
{
    public abstract class TemplateSelector : ContentControl
    {
        public abstract System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container);

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            ContentTemplate = SelectTemplate(newContent, this);
        }
    }

}
