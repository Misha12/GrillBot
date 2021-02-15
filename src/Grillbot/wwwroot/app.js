const switchPage = (pageNum, key = 'FormData.Page') => {
    const params = new URLSearchParams(location.search);

    if (params.has(key)) {
        params.delete(key);
    }

    params.append(key, pageNum);
    location.search = '?' + params.toString();
};

const clearFilter = (isPost = false) => {
    if (isPost) {
        location.reload();
    } else {
        location.search = '';
    }
};
