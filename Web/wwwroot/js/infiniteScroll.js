window.initInfiniteScroll = function (dotNetHelper) {
    window.addEventListener('scroll', () => {
        if ((window.innerHeight + window.scrollY) >= document.body.offsetHeight - 100) {
            dotNetHelper.invokeMethodAsync('OnScrollToEnd');
        }
    });
};
