﻿using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart;

public abstract class UserVarWork : WebWork
{
    public async Task @default(WebContext wc)
    {
        int uid = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users_vw WHERE id = @1");
        var o = await dc.QueryTopAsync<User>(p => p.Set(uid));

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD("用户名", o.name)._LI();
            h.LI_().FIELD("专业", User.Typs[o.typ])._LI();
            h.LI_().FIELD("电话", o.tel)._LI();
            h.LI_().FIELD("平台权限", User.Admly[o.admly])._LI();
            h.LI_().FIELD("机构权限", User.Orgly[o.suply])._LI();

            if (o.oker != null) h.LI_().FIELD2("创编", o.created, o.creator)._LI();
            if (o.adapter != null) h.LI_().FIELD2("修改", o.adapter, o.adapted)._LI();

            h._UL();

            h.TOOLBAR(bottom: true);
        });
    }
}

[Ui("我的身份权限", "账号功能")]
public class MyAccessVarWork : WebWork
{
    public void @default(WebContext wc)
    {
        var prin = (User)wc.Principal;

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            var any = 0;
            var vip = prin.vip;
            if (vip != null)
            {
                h.LI_().LABEL("大客户").SPAN_("uk-static");
                for (int i = 0; i < vip.Length; i++)
                {
                    if (i > 0)
                    {
                        h.BR();
                    }

                    var org = GrabTwin<int, int, Org>(vip[i]);
                    if (org != null)
                    {
                        h.T(org.name);
                    }
                }

                h._SPAN();
                h._LI();

                any++;
            }

            if (prin.admly > 0)
            {
                h.LI_().FIELD(User.Admly[prin.admly], "平台")._LI();

                any++;
            }

            if (prin.suply > 0 && prin.supid > 0)
            {
                var org = GrabTwin<int, int, Org>(prin.supid);

                h.LI_().FIELD(User.Orgly[prin.suply], org.name)._LI();

                any++;
            }

            if (prin.rtlly > 0 && prin.rtlid > 0)
            {
                var org = GrabTwin<int, int, Org>(prin.rtlid);

                h.LI_().FIELD(User.Orgly[prin.rtlly], org.name)._LI();

                any++;
            }

            if (any == 0)
            {
                h.LI_().FIELD(null, "暂无特殊权限")._LI();
            }

            h._UL();

            h.TOOLBAR(bottom: true);

            // spr and rvr
        }, false, 12);
    }

    [Ui("刷新", "刷新身份权限", icon: "refresh"), Tool(ButtonConfirm)]
    public async Task refresh(WebContext wc)
    {
        int uid = wc[-1];

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE id = @1");
        var o = await dc.QueryTopAsync<User>(p => p.Set(uid));

        // resend token cookie
        wc.SetTokenCookies(o);

        wc.Give(200, shared: false, maxage: 12);
    }
}

public class AdmlyUserVarWork : UserVarWork
{
    [Ui("修改", icon: "pencil"), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        short typ;
        int id = wc[0];
        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            await dc.QueryTopAsync("SELECT typ FROM users WHERE id = @1", p => p.Set(id));
            dc.Let(out typ);

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("设置专业类型");
                h.LI_().SELECT("专业类型", nameof(typ), typ, User.Typs, required: true)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(msg))._FORM();
            });
        }
        else
        {
            var f = (await wc.ReadAsync<Form>());
            typ = f[nameof(typ)];

            using var dc = NewDbContext();
            dc.Sql("UPDATE users SET typ = @1 WHERE id = @2");
            await dc.ExecuteAsync(p => p.Set(typ).Set(id));

            wc.GivePane(200);
        }
    }

    [Ui("消息", icon: "mail"), Tool(ButtonShow)]
    public async Task msg(WebContext wc)
    {
        int id = wc[0];
        string text = null;
        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("设置专业类型");
                h.LI_().TEXT("消息", nameof(text), text, min: 2, max: 20, required: true)._LI();
                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(msg))._FORM();
            });
        }
        else // POST
        {
            using var dc = NewDbContext();
            await dc.QueryTopAsync("SELECT im FROM users WHERE id = @1", p => p.Set(id));
            dc.Let(out string im);

            var f = await wc.ReadAsync<Form>();
            text = f[nameof(text)];

            await WeixinUtility.PostSendAsync(im, text);

            wc.GivePane(200);
        }
    }
}

public class AdmlyAccessVarWork : UserVarWork
{
    [AdmlyAuthorize(User.ROL_MGT)]
    [Ui(tip: "删除此人员权限", icon: "trash"), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        int uid = wc[0];

        using var dc = NewDbContext();
        dc.Sql("UPDATE users SET admly = 0 WHERE id = @1");
        await dc.ExecuteAsync(p => p.Set(uid));

        wc.Give(204); // no content
    }
}

public class OrglyAccessVarWork : UserVarWork
{
    bool IsShop => (bool)Parent.State;

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui(tip: "删除此人员权限", icon: "trash"), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        var org = wc[-2].As<Org>();
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("UPDATE users SET ").T(IsShop ? "rtlid" : "supid").T(" = NULL, ").T(IsShop ? "rtlly" : "suply").T(" = 0 WHERE id = @1 AND ").T(IsShop ? "rtlid" : "supid").T(" = @2");
        await dc.ExecuteAsync(p => p.Set(id).Set(org.id));

        wc.Give(204); // no content
    }
}

public class RtllyVipVarWork : UserVarWork
{
    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui(tip: "删除大客户身份", icon: "trash"), Tool(ButtonConfirm)]
    public async Task rm(WebContext wc)
    {
        var org = wc[-2].As<Org>();
        short id = wc[0];

        using var dc = NewDbContext(); // NOTE array_length() of empty array is NULL
        dc.Sql("UPDATE users SET vip = CASE WHEN array_length(array_remove(vip, @1), 1) IS NULL THEN NULL ELSE array_remove(vip, @1) END WHERE id = @2");
        await dc.ExecuteAsync(p => p.Set(org.id).Set(id));

        wc.Give(204); // no content
    }
}