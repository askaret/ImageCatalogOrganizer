using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ImageCatalogOrganizer
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged(this, e);
        }

        protected void NotifyPropertyChanged<TProperty>(Expression<Func<TProperty>> expression)
        {
            var propertyName = getPropertyName(expression);

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected static string getPropertyName<TProperty>(Expression<Func<TProperty>> epxr)
        {
            var memberExpression = (MemberExpression)epxr.Body;
            return memberExpression.Member.Name;
        }
    }
}