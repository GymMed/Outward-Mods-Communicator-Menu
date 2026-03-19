using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OutwardModsCommunicatorMenu.Utility.Helpers
{
    public static class CharacterHelpers
    {
        public static Character TryToFindOtherCharacterInLobby(Character mainCharacter, string otherCharName)
        {
            Character lobbyCharacter = null;

            foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
            {
                lobbyCharacter = ps.ControlledCharacter;

                if (lobbyCharacter != null && lobbyCharacter.UID != mainCharacter.UID && string.Equals(otherCharName, lobbyCharacter.Name))
                {
                    return lobbyCharacter;
                }
            }

            return null;
        }

        public static Character TryToFindOtherCharacterInLobby(Character mainCharacter)
        {
            Character lobbyCharacter = null;

            foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
            {
                lobbyCharacter = ps.ControlledCharacter;

                if (lobbyCharacter != null && lobbyCharacter.UID != mainCharacter.UID)
                {
                    return lobbyCharacter;
                }
            }

            return lobbyCharacter;
        }

        public static bool IsCharacterInDistance(Character firstCharacter, Character secondCharacter, float minimumDistance)
        {
            float sqrDistance = (firstCharacter.transform.position - secondCharacter.transform.position).sqrMagnitude;
            float followDistanceSqr = minimumDistance * minimumDistance;

            if (sqrDistance > followDistanceSqr)
                return false;
            return true;
        }

        public static bool HasManualMovement(Character character)
        {
            if (character.CharacterControl is not LocalCharacterControl lcc)
                return false;

            // Raw intent (best signal)
            if (lcc.m_moveInput.sqrMagnitude > 0.01f)
                return true;

            return false;
        }
    }

}
