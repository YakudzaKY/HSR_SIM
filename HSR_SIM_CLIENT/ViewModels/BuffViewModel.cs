using System.Windows.Controls;
using System.Windows.Media.Imaging;
using HSR_SIM_LIB.Skills;
using Image = System.Drawing.Image;

namespace HSR_SIM_CLIENT.ViewModels;

public class BuffViewModel(Buff? buff)
{
    public string EffectDescription
    {
        get
        {
            var res=String.Empty;
            buff?.Effects.ForEach(x=>res+= $"{x.Explain()}; ") ;
            return res;
        }
    }

    public IEnumerable<Effect>? Effects => buff?.Effects??null;

    public BitmapImage ? Icon => buff?.IconImage != null ? buff?.IconImage!.ToBitmapImage() : null;

    public string? SourceUnit => buff?.SourceUnit.PrintName;
    public string? SourceObject => buff?.SourceObject.GetType().ToString();
    public string? Type => buff?.GetType().Name;
    public string? BuffType => buff?.Type.ToString();
    public string? CarrierUnit => buff?.CarrierUnit.PrintName;
    public string? Stacks =>(buff!=null)? $"current: {buff.Stack} start: {buff.Reference.Stack}":"";
    public int? MaxStacks=>buff?.MaxStack;

}