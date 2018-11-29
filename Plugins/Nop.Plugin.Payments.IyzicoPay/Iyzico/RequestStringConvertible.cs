using System;

namespace Nop.Plugin.Payments.IyzicoPay.Iyzico
{
    public interface RequestStringConvertible
    {
        String ToPKIRequestString();
    }
}
