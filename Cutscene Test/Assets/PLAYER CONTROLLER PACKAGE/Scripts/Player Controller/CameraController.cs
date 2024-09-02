using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


namespace PlayerController
{
    public class CameraController : MonoBehaviour
    {
        /************************* VARIABLES *******************/
        #region Variables

        /*** PLAYER MANAGER ***/
        #region Player Manager
        [HideInInspector] PlayerManager _manager;

        #endregion

        /*** CAMERA STATE ***/
        #region Camera State
        [Header("Camera State")]
        [SerializeField] CameraState _cameraState;

        private enum CameraState
        {
            FreeLook,
            LockOn
        }

        #endregion

        /*** FREE LOOK CAMERA ***/
        #region Freelook Cam
        [Header("Freelook Cam")]
        [Tooltip("Free Look Camera Used Throughout Exploration")]
        [SerializeField] private CinemachineFreeLook _freeLookCam;
        [SerializeField] private FreeLookCamState _freeLookCamState;

        [Space]
        [Header("Roaming Recentering")]

        [Header("Roaming Static")]
        [SerializeField] private float _freeLook_RS_timer;

        [Header("Stationary Static")]
        [SerializeField] private float _freelook_SS_timer;

        [SerializeField] public float FreeLook_SS_TimerThres;
        [SerializeField] public float FreeLook_SS_XLookThres;
        [SerializeField] public float FreeLook_SS_YLookThres;

        /** Freelook State **/
        private enum FreeLookCamState
        {
            Roaming_Recenting,
            Roaming_Static,
            Stationary_Recentering,
            Stationary_Static
        }

        #endregion

        #endregion

        /************************ METHODS **********************/
        #region Methods

        //************** START, AWAKE, UPDATE *****************//
        #region Start, Awake, Update

        private void Awake()
        {
            //player manager
            _manager = FindObjectOfType<PlayerManager>();

            //freelook setup
            #region Freelook Setup
            //assign look and follow
            _freeLookCam = GameObject.FindGameObjectWithTag("Free Look Camera").GetComponent<CinemachineFreeLook>();

            _freeLookCam.Follow = _manager._playerTransform;
            _freeLookCam.LookAt = _manager._freeLookTarget;

            #endregion

        }

        // Start is called before the first frame update
        void Start()
        {
            SetCameraState(CameraState.FreeLook);
            SetState_FreeLookCam(FreeLookCamState.Roaming_Recenting);
        }

        // Update is called once per frame
        void LateUpdate()
        {
            switch (_cameraState)
            {
                case CameraState.FreeLook:
                    FreeLookCam();
                    break;
            }
        }

        #endregion

        //******************* FREE LOOK CAM *******************//
        #region Free Look Cam

        //*** FREE LOOK STATE MACHINE ***//
        #region Free Look State Machine

        /* Free Look Controller */
        private void FreeLookCam()
        {
            switch (_freeLookCamState)
            {
                case FreeLookCamState.Roaming_Recenting:
                    FreeLookCam_RoamingRecentering();
                    break;

                case FreeLookCamState.Roaming_Static:
                    FreeLookCam_RoamingStatic();
                    break;

                case FreeLookCamState.Stationary_Recentering:
                    FreeLookCam_StationaryRecentering();
                    break;

                case FreeLookCamState.Stationary_Static:
                    FreeLookCam_StationaryStatic();
                    break;
            }

            
        }

        /* Free Look Cam State */
        private void SetState_FreeLookCam(FreeLookCamState inputState)
        {
            _freeLookCamState = inputState;
            
            switch(_freeLookCamState)
            {
                case FreeLookCamState.Roaming_Recenting:
                    FreeLookCam_SetRecentering(true);
                    break;

                case FreeLookCamState.Roaming_Static:
                    _freeLook_RS_timer = 0;
                    FreeLookCam_SetRecentering(false);
                    break;

                case FreeLookCamState.Stationary_Recentering:
                    FreeLookCam_SetRecentering(true);
                    break;

                case FreeLookCamState.Stationary_Static:
                    _freelook_SS_timer = 0f;
                    FreeLookCam_SetRecentering(false);
                    break;
            }
        }

        /* Turn Recentering On and off */
        private void FreeLookCam_SetRecentering(bool inputBool)
        {
            if (inputBool)
            {
                _freeLookCam.m_YAxisRecentering.m_enabled = true;
                _freeLookCam.m_RecenterToTargetHeading.m_enabled = true;
            }
            else
            {
                _freeLookCam.m_YAxisRecentering.m_enabled = false;
                _freeLookCam.m_RecenterToTargetHeading.m_enabled = false;
            }
        }

        #endregion

        //** Free Look Camera State Controllers **//
        #region Free Look Camera State Controllers 

        /* Free Look Cam Roaming Recentering State */
        private void FreeLookCam_RoamingRecentering()
        {
            //Roaming Recentering -> Roaming Static
            #region RR -> RS
            //if the player is moving +
            //if the player is looking +
            //if above the look thres
            if(_manager._input.move.magnitude > 0 && 
               _manager._input.look.magnitude > 0)
            {
                //set state to RS
                SetState_FreeLookCam(FreeLookCamState.Roaming_Static);

                //reset RS Timer
                _freeLook_RS_timer = 0f;
            }

            #endregion

            //Roaming Recentering -> Stationary Recentering
            #region RR -> SR
            if(_manager._input.move.magnitude == 0)
            {
                SetState_FreeLookCam(FreeLookCamState.Stationary_Recentering);
            }

            #endregion
        }


        /* Free Look Cam Roaming Recentering State */
        private void FreeLookCam_RoamingStatic()
        {
            //increment timer
            _freeLook_RS_timer += Time.deltaTime;

            //reset timer if look input
            if(_manager._input.look.magnitude > _manager._data.FreeLook_RS_LookInputThres)
            {
                _freeLook_RS_timer = 0;
            }

            //Roaming Static -> Roaming Recentering
            #region RS -> RR
            //if timer is over limit &&
            //if moving
            if (_freeLook_RS_timer >= _manager._data.FreeLook_RS_TimerThres &&
                _manager._input.move.magnitude >= 0.1f)
            {
                //set state
                SetState_FreeLookCam(FreeLookCamState.Roaming_Recenting);
            }

            #endregion

            //Roaming Static -> Stationary Static
            #region RS -> SS
            //if !moving
            if (_manager._input.move.magnitude <= 0.1f)
            {
                //set state
                SetState_FreeLookCam(FreeLookCamState.Stationary_Static);
            }

            #endregion
        }


        /* Free Look Cam Stationary Recentering State */
        private void FreeLookCam_StationaryRecentering()
        {
            //Startionary Recentering to Roaming Recentering
            #region SR -> RR
            //if movement input
            if (_manager._input.move.magnitude > 0f)
            {
                //set state to roaming
                SetState_FreeLookCam(FreeLookCamState.Roaming_Recenting);
            }

            #endregion

            //Stationary Recentering to Stationary static
            #region SR -> SS
            //Set freelook cam state to stationary static if look movement detected after pause
            if (_manager._input.look.magnitude > 0.1f)
            {
                //set state
                SetState_FreeLookCam(FreeLookCamState.Stationary_Static);
            }

            #endregion
        }


        /* Free Look Cam Stationary Static State */
        private void FreeLookCam_StationaryStatic()
        {
            //increment timer
            _freelook_SS_timer += Time.deltaTime;

            //reset timer if look input
            if(_manager._input.look.magnitude >= 0.1f)
            {
                _freelook_SS_timer = 0;
            }

            //Stationary Static to Roaming Static
            #region SS -> RS
            if(_manager._input.move.magnitude > 0.1f)
            {
                //set state
                SetState_FreeLookCam(FreeLookCamState.Roaming_Static);
            }

            #endregion

            //Stationary Static to Stationary Recentering
            #region SS -> SR
            if(_freelook_SS_timer >= FreeLook_SS_TimerThres &&
              (_freeLookCam.m_YAxis.Value <= 0.5f + FreeLook_SS_YLookThres && _freeLookCam.m_YAxis.Value >= 0.5f - FreeLook_SS_YLookThres) &&
              (_freeLookCam.m_XAxis.Value <= FreeLook_SS_XLookThres && _freeLookCam.m_XAxis.Value >= -FreeLook_SS_XLookThres))
            {
                //set state
                SetState_FreeLookCam(FreeLookCamState.Stationary_Recentering);
            }



            #endregion
        }

        #endregion

        public void TriggerRecenter()
        {
            switch (_freeLookCamState)
            {
                case FreeLookCamState.Roaming_Static:
                    FreeLookCam_SetRecentering(true);
                    SetState_FreeLookCam(FreeLookCamState.Roaming_Recenting);
                    break;

                case FreeLookCamState.Stationary_Static:
                    FreeLookCam_SetRecentering(true);
                    SetState_FreeLookCam(FreeLookCamState.Stationary_Recentering);
                    break;
            }
        }
        #endregion

        //******************* INFRASTRUCTURE *****************//
        #region Infrastructure
        private void SetCameraState(CameraState inputState)
        {
            //set camera state
            _cameraState = inputState;

            //set all cam priority to 0
            SetCameraPriorities(0);

            //additional setup
            switch (inputState)
            {
                case CameraState.FreeLook:
                    _freeLookCam.Priority = 1;


                    break;


            }
        }

        private void SetCameraPriorities(int inputPriority)
        {
            _freeLookCam.Priority = inputPriority;
        }




        #endregion


        #endregion

    }
}