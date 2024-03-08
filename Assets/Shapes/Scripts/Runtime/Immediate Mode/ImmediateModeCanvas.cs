// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Rendering;

	[ExecuteAlways] [RequireComponent( typeof(Canvas) )]
	public class ImmediateModeCanvas : ImmediateModeShapeDrawer {

		Canvas canvas;
		Canvas Canvas => canvas = canvas != null ? canvas : GetComponent<Canvas>();
		RectTransform canvasRectTf;
		RectTransform CanvasRectTf => canvasRectTf = canvasRectTf != null ? canvasRectTf : GetComponent<RectTransform>();
		Camera camUI;
		Camera CamUI => camUI = camUI != null ? camUI : Canvas.worldCamera;

		List<ImmediateModePanel> panels = new List<ImmediateModePanel>();

		public void Add( ImmediateModePanel panel ) => panels.Add( panel );
		public void Remove( ImmediateModePanel panel ) => panels.Remove( panel );

		const double DEG_TO_RAD = 0.0174532925199432957692369076848861271344287188854172545609719144;

		protected void DrawPanels() {
			foreach( ImmediateModePanel panel in panels ) {
				panel.DrawPanel();
			}
		}

		public override void DrawShapes( Camera cam ) {
			if( Canvas.enabled == false )
				return;
			// this works for the scene view canvas
			using( Draw.Command( cam ) ) {
				Draw.ZTest = CompareFunction.Always;
				Draw.Matrix = GetCanvasToWorldMatrix( cam );
				DrawCanvasShapes( CanvasRectTf.rect );
			}
		}

		public bool IsCameraBasedUI => Canvas.worldCamera != null && ( Canvas.renderMode == RenderMode.ScreenSpaceCamera || Canvas.renderMode == RenderMode.WorldSpace );

		bool CanUseSimpleCameraMatrix( Camera cam ) => cam.cameraType == CameraType.SceneView || ( IsCameraBasedUI && cam == Canvas.worldCamera );

		Matrix4x4 GetCanvasToWorldMatrix( Camera cam ) {
			if( CanUseSimpleCameraMatrix( cam ) )
				return Canvas.transform.localToWorldMatrix;

			// overlay cameras are a little more complicated,
			// we have to construct a canvasToWorld matrix
			float planeDistance = ( cam.nearClipPlane + cam.farClipPlane ) / 2;
			RectTransform rtf = (RectTransform)Canvas.transform;
			Transform camTf = cam.transform;
			Vector3 forward = camTf.forward;
			Vector3 origin = camTf.TransformPoint( 0, 0, planeDistance );

			float scale = 1;
			// if perspective, then
			if( cam.orthographic ) {
				scale = 2 * cam.orthographicSize / rtf.sizeDelta.y;
			} else {
				double vFovHalfRad = ( cam.fieldOfView * DEG_TO_RAD ) / 2.0;
				double halfYSize = (float)( planeDistance * Math.Tan( vFovHalfRad ) );
				scale = (float)( ( 2 * halfYSize ) / rtf.sizeDelta.y );
			}

			Vector3 rightScale = camTf.right * scale;
			Vector3 upScale = camTf.up * scale;
			Vector3 frwScale = forward * scale; // todo
			return new Matrix4x4( rightScale, upScale, frwScale, new Vector4( origin.x, origin.y, origin.z, 1 ) );
		}

		/// <summary>The method to override in order to draw immediate mode shapes.
		/// Note: This is called from an existing Draw.Command context</summary>
		/// <param name="rect">The full region the canvas is drawn to, in UI coordinates. Usually this is the full screen rect</param>
		public virtual void DrawCanvasShapes( Rect rect ) {}

	}

}