using DotVVM.Framework.Binding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Javascript;
using System.Reflection;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Compilation.ControlTree;
using System.Linq.Expressions;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A common base for all controls that operate on collection.
    /// </summary>
    public abstract class ItemsControl : HtmlGenericControl
    {
        /// <summary>
        /// Gets or sets the source collection or a GridViewDataSet that contains data in the control.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        [BindingCompilationRequirements(
            required: new[] { typeof(DataSourceAccessBinding) },
            optional: new[] { typeof(DataSourceLengthBinding) })]
        public object DataSource
        {
            get { return GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public static readonly DotvvmProperty DataSourceProperty =
            DotvvmProperty.Register<object, ItemsControl>(t => t.DataSource, null);

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsControl"/> class.
        /// </summary>
        public ItemsControl()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsControl"/> class.
        /// </summary>
        public ItemsControl(string tagName) : base(tagName)
        {
        }

        /// <summary>
        /// Gets the data source binding.
        /// </summary>
        protected IValueBinding GetDataSourceBinding()
        {
            var binding = GetValueBinding(DataSourceProperty);
            if (binding == null)
            {
                throw new DotvvmControlException(this, $"The DataSource property of the '{GetType().Name}' control must be set!");
            }
            return binding;
        }

        protected IValueBinding GetItemBinding()
        {
            return GetForeachDataBindExpression().CastTo<ValueBindingExpression>().GetListIndexer();
        }

        public IEnumerable GetIEnumerableFromDataSource() =>
            (IEnumerable)GetForeachDataBindExpression().Evaluate(this);

        protected IValueBinding GetForeachDataBindExpression() =>
            (IValueBinding)GetDataSourceBinding().GetProperty<DataSourceAccessBinding>().Binding;

        protected string GetPathFragmentExpression() =>
            GetDataSourceBinding().GetKnockoutBindingExpression(this);

        [ApplyControlStyle]
        public static void OnCompilation(ResolvedControl control, BindingCompilationService bindingService)
        {
            if (!control.HasProperty(DataContextProperty)) return;

            var dcChange = ControlTreeResolverBase.ApplyContextChange(control.DataContextTypeStack,
                new DataContextChangeAttribute[] { new ControlPropertyBindingDataContextChangeAttribute(nameof(DataSource)), new CollectionElementDataContextChangeAttribute(0) },
                control, null);
            var dataContext = DataContextStack.Create(ResolvedTypeDescriptor.ToSystemType(dcChange.type), control.DataContextTypeStack, extenstionParameters: dcChange.extensionParameters);
            control.SetProperty(new ResolvedPropertyBinding(Internal.CurrentIndexBindingProperty,
                new ResolvedBinding(bindingService, new Compilation.BindingParserOptions(typeof(ValueBindingExpression)), dataContext,
                parsedExpression: Expression.Parameter(typeof(int), "_index").AddParameterAnnotation(
                    new BindingParameterAnnotation(dataContext, new CurrentCollectionIndexExtensionParameter())))));
        }
    }
}