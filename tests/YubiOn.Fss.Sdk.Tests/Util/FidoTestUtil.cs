using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;
using YubiOn.Fss.Sdk.Result;

namespace YubiOn.Fss.Sdk.Tests.Util;

public static class FidoTestUtil
{
    public static FinishRegisterCredentialResponse CreateCredential(YubiOnFssSdk sdk, UserDataRegisterParameter user)
    {
        var authenticator = new AuthenticatorEmulator(sdk.RpId!);
        var resp = sdk.StartRegisterCredential(new Fido2StartRegisterParameter(
            new Fido2CreationOptionsBase
            {
                Attestation = AttestationConveyancePreference.None,
                AuthenticatorSelection = new AuthenticatorSelectionCriteria
                {
                    UserVerification = UserVerificationRequirement.Required,
                    ResidentKey = ResidentKeyRequirement.Preferred
                }
            },
            user,
            new Fido2RegisterOptions(true, false, null, null)
        ));

        var startRegisterResp = resp.Data;
        var clientResp = authenticator.MakeCred("https://" + sdk.RpId, startRegisterResp.CreationOptions!);

        var finishParam = new Fido2FinishRegisterParameter
        {
            CreateResponse = new Fido2FinishRegisterParameter.CreateResponseClass
            {
                AttestationResponse = clientResp,
                Transports = new[] { "usb" }
            }
        };

        return sdk.FinishRegisterCredential(finishParam, resp.SessionCookie!);
    }
}
