using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class SceneGeneration : MonoBehaviour
{
    private Color buttonDefaultColor;
    public GameObject debugCanvas;
    private List<GameObject> sceneListButtons = new List<GameObject>();
    private List<GameObject> relationListButtons = new List<GameObject>();
    private List<GameObject> heapListButtons = new List<GameObject>();
    public GameObject scenesContentComponent;
    public GameObject relationsContentComponent;
    public GameObject heapContentComponent;
    public GameObject listButtonPrefab;
    public TMP_Text textScene1;
    public TMP_Text textScene2;
    public TMP_Text textEntity1;
    public TMP_Text textEntity2;
    public TMP_Text textRelation;
    public TMP_Text textCollision;
    public TMP_Text textState;

    public GameObject loadingCanvas;
    public Text progressText;
    public Slider progressBar;

    private ShapeNetHandler snh;
    public GameObject defaultEntity;
    private enum State {Start, Selection, SetupScenes, Processing, Placed, Generated, Completed};
    private State currState = State.Start;
    private List<Scene> scenes = new List<Scene>();

    private int scene1MaxIndex;
    private int scene2MaxIndex;
    private List<Relation> maxRelations;
    private MinHeap posHeap;
    private Vector3 initPos;
    private Entity tailEntity;
    private static float currMaxDistancePP = 0f;
    private static float currMaxDistanceNN = 0f;
    private static float currMaxDistancePN = 0f;
    private static float currMaxDistanceNP = 0f;
    private float STEP_LENGTH = 0.1f;

    public Transform userPointer;

    private List<Relation> getSceneRelations(Scene scene1, Scene scene2)
    {
        List<Relation> result = new List<Relation>();
        foreach (Relation relation in GlobalVariable.relationsScene)
        {
            if (relation.definition != null)
            {
                if (scene1.containsEntityInstance(relation.head) && scene2.containsEntityInstance(relation.tail))
                {
                    result.Add(relation);
                }
                else if (scene1.containsEntityInstance(relation.tail) && scene2.containsEntityInstance(relation.head))
                {
                    result.Add(relation);
                }
            }
        }
        return result;
    }

    private void mergeScenes(int originSceneIndex, int targetSceneIndex)
    {
        Scene origin = scenes[originSceneIndex];
        Scene target = scenes[targetSceneIndex];
        foreach (Entity entity in origin.entities)
        {
            entity.model.transform.parent.parent = target.sceneBucket.transform;
            target.entities.Add(entity);
        }
        Destroy(origin.sceneBucket);
        scenes.RemoveAt(originSceneIndex);
    }

    private void requiredNewPos(Vector3 vec, float currMaxDistance, Vector3 startVec)
    {
        if (vec.x >= currMaxDistance || vec.y >= currMaxDistance)
        {
            do{
                currMaxDistance += STEP_LENGTH;
                for (float direction = -currMaxDistance; direction < currMaxDistance; direction += STEP_LENGTH)
                {
                    requiredNewPosHelper(new Vector3(currMaxDistance, 0, direction), currMaxDistance, startVec);
                    requiredNewPosHelper(new Vector3(-currMaxDistance, 0, direction), currMaxDistance, startVec);
                    requiredNewPosHelper(new Vector3(direction, 0, currMaxDistance), currMaxDistance, startVec);
                    requiredNewPosHelper(new Vector3(direction, 0, -currMaxDistance), currMaxDistance, startVec);
                }
            }while(posHeap.heapList.Where(x => x.value.y <= 0.001).ToList().Count == 0);
        }
    }

    private void requiredNewPosHelper(Vector3 addPos, float currMaxDistance, Vector3 startVec)
    {
        float errorValue = 0;
        float maxY = 0;
        bool addZero = false;
        GameObjectHelper.GameObjectBoundries tailBoundries = GameObjectHelper.getGameObjectBoundries(tailEntity.model);
        //find ground
        foreach (Scene scene in scenes)
        {
            if (scene.inScene)
            {
                foreach (Entity entity in scene.entities)
                {
                    if (!GameObject.ReferenceEquals(tailEntity.model, entity.model))
                    {
                        if (GameObjectHelper.objectsOverlappingOnHeight(entity.model, tailEntity.model, addPos + startVec))
                        {
                            GameObjectHelper.GameObjectBoundries headBoundries = GameObjectHelper.getGameObjectBoundries(entity.model);
                            maxY = Mathf.Max(maxY, headBoundries.y_max);
                            if (headBoundries.y_max - tailBoundries.y_min - (initPos.y - tailEntity.model.transform.position.y) == 0)
                            {
                                addZero = true;
                            }
                        }
                    }
                }
            }
        }
        
        float allignY = maxY - tailBoundries.y_min - (initPos.y - tailEntity.model.transform.position.y);
        addPos.y = allignY;

        Vector3 newPos = initPos + addPos + startVec;
        foreach (Relation relation in maxRelations)
        {
            // maybe include head as well
            if (GameObject.ReferenceEquals(tailEntity.model, relation.tail.model))
            {
                errorValue += relation.calcErrorValue(newPos);
            }
        }
        posHeap.addElement(addPos, errorValue, currMaxDistance, startVec);

        if (addZero)
        {
            Vector3 addPosZero = new Vector3(addPos.x, 0, addPos.z);

            float errorValueZero = 0;

            foreach (Relation relation in maxRelations)
            {
                // maybe include head as well
                if (GameObject.ReferenceEquals(tailEntity.model, relation.tail.model))
                {
                    errorValueZero += relation.calcErrorValue(addPosZero);
                }
            }
            posHeap.addElement(addPosZero, errorValueZero, currMaxDistance, startVec);
        }
    }

    private void applySceneObjectItems(Scene scene)
    {
        var initGround = new {mid_x = 500, mid_z = 500, ground_y = 0};

        if (!scene.inScene)
        {
            scene.inScene = true;
            foreach (Entity entity in scene.entities)
            {
                float y_min = GameObjectHelper.getGameObjectBoundries(entity.model).y_min;
                Vector3 newPos = new Vector3(initGround.mid_x, initGround.ground_y + (entity.model.transform.position.y - y_min), initGround.mid_z);
                entity.model.transform.position = newPos;

                GameObject gameObject = GameObjectHelper.getAllChildGameObjects(entity.ghostModel)[0];
                gameObject.tag = "inScene";
                gameObject.AddComponent<GameObjectCollision>();
                Rigidbody rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                MeshCollider mc = gameObject.AddComponent<MeshCollider>();
                mc.convex = true;
            }
        }
    }

    private void runStart()
    {
        if (Settings.customAttributes)
        {
            foreach (Entity entity in GlobalVariable.entitiesScene)
            {
                List<string> attributes = DragDropParameters.currScene.getAllAttributesForIndex(entity.index);
                //check if attribute is already annotated for the used model and apply custom one if not
                foreach (string attribute in attributes)
                {
                    if (!entity.modelInfo.ContainsTag(attribute))
                    {
                        GameObject gameObject = GameObjectHelper.getAllChildGameObjects(entity.model)[0];
                        ApplyAttributes.applyOne(gameObject, attribute);
                    }
                }
            }
        }

        currState = State.Selection;
        int sceneCounter = 0;
        foreach (Entity entity in GlobalVariable.entitiesScene)
        {
            // create ghost model
            GameObject ghost = GameObject.Instantiate(entity.model);
            entity.ghostModel = ghost;

            // add to scene
            Scene scene = new Scene(sceneCounter);
            scene.AddEntity(entity);
            scenes.Add(scene);
            sceneCounter++;
        }
    }

    private void runSelection()
    {
        int currMax = -1;
        if (scenes.Count >= 2){
            for (int index1 = 0; index1 < scenes.Count; index1++)
            {
                for (int index2 = index1+1; index2 < scenes.Count; index2++)
                {
                    Scene currScene1 = scenes[index1];
                    Scene currScene2 = scenes[index2];
                    List<Relation> currRelations = getSceneRelations(currScene1, currScene2);
                    int amountRelations = currRelations.Count;
                    if (amountRelations > currMax)
                    {
                        currMax = amountRelations;
                        scene1MaxIndex = index1;
                        scene2MaxIndex = index2;
                        maxRelations = currRelations;
                    }
                }
            } 
            GlobalVariable.currMaxDistancePP = 0f;
            GlobalVariable.currMaxDistanceNN = 0f;
            GlobalVariable.currMaxDistancePN = 0f;
            GlobalVariable.currMaxDistanceNP = 0f;
            currState = State.SetupScenes;
        }
        else
        {
            currState = State.Generated;
        }
    }

    private void runSetupScenes()
    {
        posHeap = new MinHeap();
        Scene scene1 = scenes[scene1MaxIndex];
        Scene scene2 = scenes[scene2MaxIndex];
        applySceneObjectItems(scene1);
        applySceneObjectItems(scene2);
        if (maxRelations.Count > 0)
        {
            Relation firstRelation = maxRelations[0];
            if (firstRelation.definition.useRotation)
            {
                firstRelation.head.model.transform.parent.rotation = Quaternion.Euler(firstRelation.definition.head.rotation.getVector3());
                firstRelation.tail.model.transform.parent.rotation = Quaternion.Euler(firstRelation.definition.tail.rotation.getVector3());
            }
            if (firstRelation.definition.useScale)
            {
                Vector3 headScale = firstRelation.definition.head.scale.getVector3();
                Vector3 tailScale = firstRelation.definition.tail.scale.getVector3();
                Vector3 oldScale = firstRelation.tail.model.transform.parent.localScale;
                Vector3 newScale = new Vector3(oldScale.x*tailScale.x/headScale.x, oldScale.y*tailScale.y/headScale.y, oldScale.z*tailScale.z/headScale.z);
                firstRelation.tail.model.transform.parent.localScale = newScale;
            }
            initPos = firstRelation.getInitialTailPosition(false, false);
            tailEntity = firstRelation.tail;
            tailEntity.model.transform.position = initPos;
            
            requiredNewPos(Vector3.zero, currMaxDistancePP, Vector3.zero);

            //rotations
            if (!firstRelation.definition.useRotation)
            {
            Vector3 negativeNegative = firstRelation.getInitialTailPosition(true, true);
            requiredNewPos(Vector3.zero, currMaxDistanceNN, negativeNegative - initPos);
            Vector3 negativePositive = firstRelation.getInitialTailPosition(true, false);
            requiredNewPos(Vector3.zero, currMaxDistanceNP, negativePositive - initPos);
            Vector3 positiveNegative = firstRelation.getInitialTailPosition(false, true);
            requiredNewPos(Vector3.zero, currMaxDistancePN, positiveNegative - initPos);
            }
        } 
        GlobalVariable.currMaxDistancePP = 0f;
        GlobalVariable.currMaxDistanceNN = 0f;
        GlobalVariable.currMaxDistancePN = 0f;
        GlobalVariable.currMaxDistanceNP = 0f;

        scene1.setAllGhostObjectPos();
        scene2.setAllGhostObjectPos();

        GlobalVariable.collisionOccured = false;
        currState = State.Placed;
    }

    private void runProcessing()
    {
        MinHeap.Element addElem = posHeap.extractMin();
        requiredNewPos(addElem.value, addElem.currMaxDistance, addElem.startVec);
        Vector3 newPos = initPos + addElem.value + addElem.startVec;
        tailEntity.model.transform.position = newPos;

        Scene scene1 = scenes[scene1MaxIndex];
        Scene scene2 = scenes[scene2MaxIndex];
        scene1.setAllGhostObjectPos();
        scene2.setAllGhostObjectPos();

        currState = State.Placed;
        GlobalVariable.collisionOccured = false;
    }

    private void runPlaced()
    {
        if (GlobalVariable.collisionOccured)
        {
            currState = State.Processing;
        }
        else
        {
            mergeScenes(scene1MaxIndex, scene2MaxIndex);
            currState = State.Selection;
        }
    }

    private void runGenerated()
    {
        foreach (Entity entity in GlobalVariable.entitiesScene)
        {
            Destroy(entity.ghostModel);
        }

        //center scene
        GameObject finalScene = scenes[0].sceneBucket;
        finalScene.transform.position = GameObjectHelper.centerObjectOnGroundPoint(finalScene, new Vector3(500, 0, 500));

        if (GlobalVariable.physicsActivated)
        {
            foreach (Entity entity in GlobalVariable.entitiesScene)
            {
                GameObject gameObject = GameObjectHelper.getAllChildGameObjects(entity.model)[0];
                Rigidbody rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = true;
                rb.mass = entity.modelInfo.staticFrictionForce;
                MeshCollider mc = gameObject.AddComponent<MeshCollider>();
                mc.convex = true;
                ObjectInteraction oi = gameObject.AddComponent<ObjectInteraction>();
                oi.userPointer = userPointer;
                gameObject.tag = "inScene";
            }
        }
        currState = State.Completed;
    }

    private void setDebugInformation()
    {
        ColorUtility.TryParseHtmlString("#383838", out buttonDefaultColor);
        // scene components
        try{
            while (sceneListButtons.Count != 0)
            {
                GameObject button = sceneListButtons[0];
                sceneListButtons.RemoveAt(0);
                Destroy(button);
            }

            for (int scene_nr = 0; scene_nr < scenes.Count; scene_nr++)
            {
                Scene scene = scenes[scene_nr];
                GameObject listButton =  Instantiate(listButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                listButton.GetComponentInChildren<TMP_Text>().text = new string(' ', 2) + scene.sceneBucket.name;
                listButton.transform.parent = scenesContentComponent.transform;
                listButton.transform.localScale = new Vector3(1f, 1f, 1f);
                sceneListButtons.Add(listButton);
            }
        }
        catch(Exception e)
        {}
        // relation components
        try{
            while (relationListButtons.Count != 0)
            {
                GameObject button = relationListButtons[0];
                relationListButtons.RemoveAt(0);
                Destroy(button);
            }
            for (int relation_nr = 0; relation_nr < GlobalVariable.relationsScene.Count; relation_nr++)
            {
                Relation relation = GlobalVariable.relationsScene[relation_nr];
                GameObject listButton =  Instantiate(listButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                string headIndex = '(' + relation.head.index.ToString() + (relation.head.copyNr == -1 ? "" : '-' + relation.head.copyNr.ToString()) + ')';
                string tailIndex = '(' + relation.tail.index.ToString() + (relation.tail.copyNr == -1 ? "" : '-' + relation.tail.copyNr.ToString()) + ')';
                listButton.GetComponentInChildren<TMP_Text>().text = new string(' ', 2) + relation.head.entityName + headIndex + ' ' + relation.relationType + ' ' + relation.tail.entityName + tailIndex;
                listButton.transform.parent = relationsContentComponent.transform;
                listButton.transform.localScale = new Vector3(1f, 1f, 1f);
                relationListButtons.Add(listButton);
            }
        }
        catch(Exception e)
        {}
        // heap componentes
        try{
            while (heapListButtons.Count != 0)
            {
                GameObject button = heapListButtons[0];
                heapListButtons.RemoveAt(0);
                Destroy(button);
            }
            for (int heap_nr = 0; heap_nr < posHeap.heapList.Count; heap_nr++)
            {
                MinHeap.Element element = posHeap.heapList[heap_nr];
                GameObject listButton =  Instantiate(listButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                listButton.GetComponentInChildren<TMP_Text>().text = new string(' ', 2) + element.value.ToString() + " - P:" + element.priority;
                listButton.transform.parent = heapContentComponent.transform;
                listButton.transform.localScale = new Vector3(1f, 1f, 1f);
                heapListButtons.Add(listButton);
            }
        }
        catch(Exception e)
        {}
        // current components
        try{
            textScene1.text = "Scene1:\n" + scenes[scene1MaxIndex].sceneBucket.name;
        }
        catch(Exception e)
        {
            textScene1.text = e.Message;
        }
        try{
            textScene2.text = "Scene2:\n" + scenes[scene2MaxIndex].sceneBucket.name;
        }
        catch(Exception e)
        {
            textScene2.text = e.Message;
        }
        try{
            string currHeadIndex = '(' + maxRelations[0].head.index.ToString() + (maxRelations[0].head.copyNr == -1 ? "" : '-' + maxRelations[0].head.copyNr.ToString()) + ')';
            string currTailIndex = '(' + maxRelations[0].tail.index.ToString() + (maxRelations[0].tail.copyNr == -1 ? "" : '-' + maxRelations[0].tail.copyNr.ToString()) + ')';
            textRelation.text = "Relation:\n" + maxRelations[0].head.entityName + currHeadIndex + ' ' + maxRelations[0].relationType + ' ' + maxRelations[0].tail.entityName + currTailIndex;
            textEntity1.text = "Entity1:\n" + maxRelations[0].head.entityName + currHeadIndex;
            textEntity2.text = "Entity2:\n" + maxRelations[0].tail.entityName + currTailIndex;
        }
        catch(Exception e)
        {
            textRelation.text = e.Message;
            textEntity1.text = e.Message;
            textEntity2.text = e.Message;
        }
        textCollision.text = "Collision:\n" + GlobalVariable.collisionOccured.ToString();
        textState.text = "State:\n" + currState.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        snh = gameObject.AddComponent<ShapeNetHandler>();
        snh.usedFor = ShapeNetHandler.Usage.SceneGeneration;
        snh.defaultEntity = defaultEntity;
        debugCanvas.SetActive(Settings.debugMode);
        loadingCanvas.SetActive(!Settings.debugMode);
        GlobalVariable.currMaxDistancePP = 0f;
        GlobalVariable.currMaxDistanceNN = 0f;
        GlobalVariable.currMaxDistancePN = 0f;
        GlobalVariable.currMaxDistanceNP = 0f;
    }
    int waitForSwitchCounter = 0;
    int waitUntil = 5;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            SceneManager.LoadScene("MainMenu");
            GlobalVariable.scenesForVisualization = null;
            GlobalVariable.inputText = null;
            GlobalVariable.entitiesScene = new List<Entity>();
            GlobalVariable.relationsScene = new List<Relation>();
            GlobalVariable.sceneObjectsLoaded = false;
            DragDropParameters.currScene = null;
        }
        bool stateSwitchable = false;
        if (Input.GetKeyDown("space") && Settings.debugMode)
        {
            stateSwitchable = true;
        } 
        if (!Settings.debugMode)
        {
            if (waitForSwitchCounter > waitUntil)
            {
                stateSwitchable = true;
                waitForSwitchCounter = 0;
            }
            waitForSwitchCounter++;
        }
        if (stateSwitchable)
        {
            if (GlobalVariable.sceneObjectsLoaded && currState == State.Start)
            {
                runStart();
            }
            else if (currState == State.Selection)
            {
                runSelection();
            }
            else if (currState == State.SetupScenes)
            {
                runSetupScenes();
            }
            else if (currState == State.Processing)
            {
                runProcessing();
            }
            else if (currState == State.Placed)
            {
                runPlaced();
            }
            else if (currState == State.Generated)
            {
                runGenerated();
            }
        }
        else
        {
            if (currState == State.Processing || currState == State.Placed)
            {
                Scene scene1 = scenes[scene1MaxIndex];
                Scene scene2 = scenes[scene2MaxIndex];
                scene1.setAllGhostObjectPos();
                scene2.setAllGhostObjectPos();
            }
        }
        if (Settings.debugMode && Input.GetKeyDown("space"))
        {
            setDebugInformation();
        }
        if (!Settings.debugMode && (currState == State.Placed || currState == State.Completed))
        {
            if (currState == State.Completed)
            {
                loadingCanvas.SetActive(false);
            }
            else
            {
                int percent = (GlobalVariable.entitiesScene.Count - scenes.Count)*100/GlobalVariable.entitiesScene.Count;
                progressText.text = percent + "%";
                progressBar.value = percent;
            }
        }
    }
}
