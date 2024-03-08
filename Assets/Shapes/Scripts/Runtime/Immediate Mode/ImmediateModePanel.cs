// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	using UnityEngine;

	[ExecuteAlways]
	public class ImmediateModePanel : MonoBehaviour {

		public bool drawRelativeToPanel = true;
		public bool useDrawingScope = true;

		ImmediateModeCanvas imCanvas;
		ImmediateModeCanvas ImCanvas => imCanvas != null ? imCanvas : ( imCanvas = GetComponentInParent<ImmediateModeCanvas>() );
		public bool Valid => ImCanvas != null;

		public virtual void OnEnable() {
			if( Valid )
				ImCanvas.Add( this );
			else {
				Debug.LogWarning( $"{nameof(ImmediateModePanel)} attached to {gameObject.name} is missing an {nameof(ImmediateModeCanvas)} component on its canvas", this );
			}
		}

		public virtual void OnDisable() {
			if( Valid )
				ImCanvas.Remove( this );
		}

		internal void DrawPanel() {
			RectTransform tf = transform as RectTransform;

			if( useDrawingScope )
				Draw.Push();

			if( drawRelativeToPanel ) {
				Draw.PushMatrix();
				Draw.Matrix *= Matrix4x4.TRS( tf.localPosition, tf.localRotation, tf.localScale );
			}

			DrawPanelShapes( tf.rect );

			if( drawRelativeToPanel )
				Draw.PopMatrix();

			if( useDrawingScope )
				Draw.Pop();
		}

		/// <summary>The method to draw the content of this panel</summary>
		/// <param name="rect">The rect of this panel, in UI space</param>
		public virtual void DrawPanelShapes( Rect rect ) {
			// override this!
		}

	}

}