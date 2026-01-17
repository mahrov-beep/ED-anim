namespace _Project.Scripts.GameView
{
    using System;
    using Photon.Deterministic;
using Quantum;
using UnityEngine;

public enum EPreviewType
  {
    None,
    Linear,
    Ballistic,
    Angle,
  }

  public class AttackPreview : QuantumSceneViewComponent<CustomViewContext> { 
    [SerializeField] private FP _maxRotationAngle = FP.FromFloat_UNSAFE(90f);
      
    [SerializeField] private EPreviewType previewType;
    [SerializeField] private EPreviewType specialPreviewType;

    [SerializeField] private AttackPreviewDynamicMesh attackPreviewDynamicMesh;
    
    [SerializeField] private FP _maxDistance;

    [SerializeField] private GameObject _previewsContainer;

    [SerializeField] private GameObject _linearPreview;
    [SerializeField] private GameObject _ballisticPreview;
    [SerializeField] private GameObject _anglePreview;

    [SerializeField] private GameObject _ballisticPreviewCircle;

    private MeshRenderer[] _meshRenderer;
    [SerializeField] private Color _standardAimMaterialColor;
    [SerializeField] private Color _blockedMaterialColor;

    public AttackPreviewDynamicMesh AttackPreviewDynamicMesh => this.attackPreviewDynamicMesh;

    private void Awake()
    {
      _meshRenderer = GetComponentsInChildren<MeshRenderer>(true);
    }

    private void Start() {
        attackPreviewDynamicMesh.CustomViewContext = ViewContext;
    }

    private void TogglePreviews(EPreviewType previewType)
    {
      switch (previewType)
      {
        case EPreviewType.None:
          _linearPreview.SetActive(false);
          _ballisticPreview.SetActive(false);
          _anglePreview.SetActive(false);
          break;
        case EPreviewType.Linear:
          _ballisticPreview.SetActive(false);
          _ballisticPreviewCircle.SetActive(false);
          _anglePreview.SetActive(false);

          _linearPreview.SetActive(true);
          break;
        case EPreviewType.Ballistic:
          _linearPreview.SetActive(false);
          _anglePreview.SetActive(false);

          _ballisticPreview.SetActive(true);
          _ballisticPreviewCircle.SetActive(true);
          break;
        case EPreviewType.Angle:
          _linearPreview.SetActive(false);
          _ballisticPreviewCircle.SetActive(false);
          _ballisticPreview.SetActive(false);

          _anglePreview.SetActive(true);
          break;
      }
    }

    public unsafe void UpdateAttackPreview(FPVector2 aim, bool isSpecial)
    {
      if (!IsRotationWithinLimit(aim, ViewContext.LocalView.transform.forward.ToFPVector3())) {
          //TogglePreviews(EPreviewType.None);
         // return;
      }
        
      EPreviewType previewType = isSpecial ? this.specialPreviewType : this.previewType;
      TogglePreviews(previewType);

      var aimNormalized = aim.Normalized;
      transform.position = ViewContext.LocalView.transform.position + Vector3.up * .2f;

      Frame f = QuantumRunner.Default.Game.Frames.Predicted;

      if (previewType != EPreviewType.Angle)
      {
        float previewSize = previewType == EPreviewType.Linear
          ? GetPreviewSizeLinear(f, aimNormalized)
          : GetPreviewSizeBallistic(aim);
        _previewsContainer.transform.localScale = new Vector3(1, 1, previewSize);
      }
      else
      {
        _previewsContainer.transform.localScale = new Vector3(1, 1, 1);
      }

      Transform3D characterTransform = f.Get<Transform3D>(ViewContext.LocalView.EntityRef);

      if (previewType == EPreviewType.Ballistic)
      {
        var circlePosition = characterTransform.Position +
                             aim.XOY.Normalized * Mathf.Clamp(aim.Magnitude.AsFloat, 0, _maxDistance.AsFloat).ToFP();
        _ballisticPreviewCircle.transform.position =
          new Vector3(circlePosition.X.AsFloat, .2f, circlePosition.Y.AsFloat);
      }

      Unit    unit   = f.Get<Unit>(ViewContext.LocalView.EntityRef);
      Weapon weapon = f.Get<Weapon>(unit.ActiveWeaponRef);
      
      foreach (var mesh in _meshRenderer)
      {
        if (weapon.BulletsCount <= 0)
        {
          mesh.material.color = _blockedMaterialColor;
        }
        else
        {
          mesh.material.color = _standardAimMaterialColor;
        }
      }
    }
    
    private bool IsRotationWithinLimit(FPVector2 aimDirection, FPVector3 forwardDirection) {
        var angle = FPVector3.Angle(forwardDirection, aimDirection.XOY);
        
        return angle <= _maxRotationAngle;
    }

    private float GetPreviewSizeLinear(Frame frame, FPVector2 aimDirection)
    {
      Transform3D characterTransform = frame.Get<Transform3D>(ViewContext.LocalView.EntityRef);
      var hit = frame.Physics3D.Raycast(characterTransform.Position, aimDirection.XOY, _maxDistance,
        frame.Layers.GetLayerMask("Static"));
      if (hit.HasValue == false)
      {
        return _maxDistance.AsFloat;
      }

      var previewSize =
        Vector2.Distance(characterTransform.Position.ToUnityVector2(), hit.Value.Point.ToUnityVector2());
      return previewSize;
    }

    private float GetPreviewSizeBallistic(FPVector2 aim)
    {
      return Mathf.Clamp(aim.Magnitude.AsFloat, 0, _maxDistance.AsFloat);
    }
  }
}