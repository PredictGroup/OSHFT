
using System;

namespace OSHFT_Q_R.ObjItems
{
  // ************************************************************************

  struct TradeOpItem
  {
    public readonly TradeOp Value;
    public TradeOpItem(TradeOp value) { this.Value = value; }

    public override string ToString() { return ToString(Value); }

    public static string ToString(TradeOp value)
    {
      switch(value)
      {
        case TradeOp.Buy:
          return "Покупка";
        case TradeOp.Sell:
          return "Продажа";
        case TradeOp.Upsize:
          return "Наращивание";
        case TradeOp.Downsize:
          return "Уменьшение";
        case TradeOp.Close:
          return "Закрытие";
        case TradeOp.Reverse:
          return "Разворот";
        case TradeOp.Cancel:
          return "Отмена";
      }

      return value.ToString();
    }

    public static TradeOpItem[] GetItems()
    {
      return new TradeOpItem[] {
        new TradeOpItem(TradeOp.Buy),
        new TradeOpItem(TradeOp.Sell),
        new TradeOpItem(TradeOp.Upsize),
        new TradeOpItem(TradeOp.Downsize),
        new TradeOpItem(TradeOp.Close),
        new TradeOpItem(TradeOp.Reverse),
        new TradeOpItem(TradeOp.Cancel) };
    }
  }

}
