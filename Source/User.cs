﻿using ChainFx;

namespace ChainMart
{
    public class User : Entity, IKeyable<int>
    {
        public static readonly User Empty = new User();

        // pro types
        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {0, "普通"},
            {1, "市场运营师"},
            {2, "健康管理师"},
        };

        public const short
            ADMLY_ = 0b0000001, // common
            ADMLY_OPN = 0b0000011,
            ADMLY_FIN = 0b0000101,
            ADMLY_MGT = 0b0011111,
            ADMLY_SPR = 0b0111111,
            ADMLY_RVR = 0b1000001;

        public static readonly Map<short, string> Admly = new Map<short, string>
        {
            {ADMLY_OPN, "业务"},
            {ADMLY_FIN, "财务"},
            {ADMLY_MGT, "管理"},
            {ADMLY_SPR, "负责"},
            {ADMLY_RVR, "审核"},
        };

        public const short
            ORGLY_ = 0b000001, // common
            ORGLY_OPN = 0b0000011, // operation
            ORGLY_LOG = 0b0001001, // logistic
            ORGLY_MGT = 0b0011111, // manager & delegate
            ORGLY_SPR = 0b0111111, // superviser
            ORGLY_RVR = 0b1000001; // reviewer

        public static readonly Map<short, string> Orgly = new Map<short, string>
        {
            {0, null},
            {ORGLY_OPN, "业务"},
            {ORGLY_LOG, "物流"},
            {ORGLY_MGT, "管理"},
            {ORGLY_SPR, "负责"},
            {ORGLY_RVR, "审核"},
        };

        internal int id;
        internal string tel;
        internal string addr;
        internal string im;

        // later
        internal string credential;
        internal short admly;
        internal int orgid;
        internal short orgly;
        internal int vip;
        internal bool icon;

        public override void Read(ISource s, short msk = 0xff)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(tel), ref tel);
                s.Get(nameof(im), ref im);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(addr), ref addr);
                s.Get(nameof(credential), ref credential);
                s.Get(nameof(admly), ref admly);
                s.Get(nameof(orgid), ref orgid);
                s.Get(nameof(orgly), ref orgly);
                s.Get(nameof(vip), ref vip);
                s.Get(nameof(icon), ref icon);
            }
        }

        public override void Write(ISink s, short msk = 0xff)
        {
            base.Write(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(tel), tel);
            s.Put(nameof(im), im);
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(addr), addr);
                s.Put(nameof(credential), credential);
                s.Put(nameof(admly), admly);
                s.Put(nameof(orgid), orgid);
                s.Put(nameof(orgly), orgly);
                s.Put(nameof(vip), vip);
                s.Put(nameof(icon), icon);
            }
        }

        public int Key => id;

        public bool IsDelegateOf(int orgid) => orgid == this.orgid && (orgly & ORGLY_OPN) == orgly;

        public bool IsProfessional => typ >= 1;

        public bool IsAdmly => admly > 0;

        public bool IsOrgly => orgly > 0 && orgid > 0;

        public override string ToString() => name;
    }
}