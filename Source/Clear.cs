﻿using System;
using ChainFx;

namespace ChainMart
{
    public class Clear : Entity, IKeyable<int>
    {
        public static readonly Clear Empty = new Clear();

        public const short
            TYP_SUPPLY = 1,
            TYP_BUY = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_SUPPLY, "供应链"},
            {TYP_BUY, "零售"},
        };

        public const short
            STU_ = 0,
            STU_APPROVED = 2,
            STU_PAID = 3;


        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STU_, "新结算"},
            {STU_APPROVED, "已确认"},
            {STU_PAID, "已支付"},
        };

        internal int id;
        internal DateTime till;
        internal int orgid;
        internal int sprid;
        internal short orders;
        internal decimal amt;
        internal decimal rate;
        internal decimal pay;

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & MSK_EXTRA) == MSK_EXTRA)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(till), ref till);
            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(sprid), ref sprid);
            s.Get(nameof(orders), ref orders);
            s.Get(nameof(amt), ref amt);
            s.Get(nameof(rate), ref rate);
            s.Get(nameof(pay), ref pay);
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & MSK_EXTRA) == MSK_EXTRA)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(till), till);
            s.Put(nameof(orgid), orgid);
            s.Put(nameof(sprid), sprid);
            s.Put(nameof(orders), orders);
            s.Put(nameof(amt), amt);
            s.Put(nameof(rate), rate);
            s.Put(nameof(pay), pay);
        }

        public int Key => id;

        public short Status => state;
    }
}