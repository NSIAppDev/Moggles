export default function GlobalSearchModel() {
    return {
        keyword: '',
        applicationIds: [],
        progressStatus: '',
        createdStart: null,
        createdEnd: null,
        environments: [
            { name: '', enabled: null },
            { name: '', enabled: null },
            { name: '', enabled: null }
        ],
        workItemIdentifier: '',
        assignedTo: [],
        isPermanent: null,
        changedStart: null,
        changedEnd: null,
        userAccepted: null
    };
}