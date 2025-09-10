using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace TestGameMaui.Behaviors
{
    public class AnimateOnSelectedBehavior : Behavior<VisualElement>
    {
        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
            nameof(IsSelected),
            typeof(bool),
            typeof(AnimateOnSelectedBehavior),
            false,
            propertyChanged: OnIsSelectedChanged);

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        VisualElement? associated;

        protected override void OnAttachedTo(VisualElement bindable)
        {
            base.OnAttachedTo(bindable);
            associated = bindable;
        }

        protected override void OnDetachingFrom(VisualElement bindable)
        {
            base.OnDetachingFrom(bindable);
            associated = null;
        }

        static void OnIsSelectedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var behavior = bindable as AnimateOnSelectedBehavior;
            behavior?.HandleIsSelectedChanged((bool)newValue);
        }

        async void HandleIsSelectedChanged(bool isSelected)
        {
            if (associated == null)
                return;

            // Simple pulse animation when selected
            try
            {
                if (isSelected)
                {
                    await associated.ScaleTo(1.15, 120, Easing.CubicOut);
                    await associated.ScaleTo(1.0, 120, Easing.CubicIn);
                }
                else
                {
                    // ensure scale reset
                    await associated.ScaleTo(1.0, 120, Easing.Linear);
                }
            }
            catch (Exception)
            {
                // Ignore animation exceptions (e.g., during page teardown)
            }
        }
    }
}
